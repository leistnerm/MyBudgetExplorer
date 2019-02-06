/* 
 * Copyright 2019 Mark D. Leistner
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 *   
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.Extensions.Configuration;
using MyBudgetExplorer.Models.YNAB;
using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace MyBudgetExplorer.Models
{
    public static class Cache
    {
        private static readonly RegionEndpoint region;
        private static readonly string bucketName;
        private static readonly string awsAccessKey;
        private static readonly string awsSecretKey;

        static Cache()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);
            var configuration = configurationBuilder.Build();

            region = RegionEndpoint.USEast2;
            bucketName = configuration.GetSection("AWS")["Bucket"];
            awsAccessKey = configuration.GetSection("AWS")["AccessKey"];
            awsSecretKey = configuration.GetSection("AWS")["SecretKey"];
        }


        private static byte[] EncryptAES(byte[] decrypted, byte[] key, byte[] iv)
        {
            AWSXRayRecorder.Instance.BeginSubsegment("MyBudgetExplorer.Models.Cache.EncryptAES()");
            try
            {
                // Check arguments.
                if (decrypted == null || decrypted.Length <= 0)
                    throw new ArgumentNullException("decrypted");
                if (key == null || key.Length <= 0)
                    throw new ArgumentNullException("key");
                if (iv == null || iv.Length <= 0)
                    throw new ArgumentNullException("iv");

                // Create an Aes object with the specified key and IV.
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;

                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    // Create the streams used for encryption.
                    using (MemoryStream msEncrypt = new MemoryStream())
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var gs = new DeflateStream(csEncrypt, CompressionMode.Compress))
                        {
                            gs.Write(decrypted, 0, decrypted.Length);
                        }
                        return msEncrypt.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                AWSXRayRecorder.Instance.AddException(e);
                throw;
            }
            finally
            {
                AWSXRayRecorder.Instance.EndSubsegment();
            }
        }

        private static byte[] DecryptAES(byte[] encrypted, byte[] key, byte[] iv)
        {
            AWSXRayRecorder.Instance.BeginSubsegment("MyBudgetExplorer.Models.Cache.DecryptAES()");
            try
            {
                // Check arguments.
                if (encrypted == null || encrypted.Length <= 0)
                    throw new ArgumentNullException("encrypted");
                if (key == null || key.Length <= 0)
                    throw new ArgumentNullException("key");
                if (iv == null || iv.Length <= 0)
                    throw new ArgumentNullException("iv");

                // Create an Aes object with the specified key and IV.
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;

                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    // Create the streams used for decryption.
                    using (MemoryStream msDecrypt = new MemoryStream(encrypted))
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var gs = new DeflateStream(csDecrypt, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[16 * 1024];
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int read;
                            while ((read = gs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ms.Write(buffer, 0, read);
                            }
                            return ms.ToArray();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AWSXRayRecorder.Instance.AddException(e);
                throw;
            }
            finally
            {
                AWSXRayRecorder.Instance.EndSubsegment();
            }
        }

        public static BudgetDetail GetLocalBudget(string userId)
        {
            AWSXRayRecorder.Instance.BeginSubsegment("MyBudgetExplorer.Models.Cache.GetLocalBudget()");
            try
            {
                var binaryFormatter = new BinaryFormatter();
                var aesKey = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(userId));
                var aesIV = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(userId));
                var hash = BitConverter.ToString(aesKey).Replace("-", "").ToLower();
                var fileName = $"budget.{hash}";
                var tempFilePath = Path.Combine(Path.GetTempPath(), fileName);

                BudgetDetail budget = null;
                if (File.Exists(tempFilePath))
                {
                    try
                    {
                        var decrypted = DecryptAES(File.ReadAllBytes(tempFilePath), aesKey, aesIV);
                        using (var ms = new MemoryStream(decrypted))
                        {
                            budget = (BudgetDetail)binaryFormatter.Deserialize(ms);
                        }
                    }
                    catch
                    {
                        AWSXRayRecorder.Instance.AddAnnotation("Budget-Local-Cache", "File found, deserialize failed.");
                        File.Delete(tempFilePath);
                    }
                }


                return budget;
            }
            finally
            {
                AWSXRayRecorder.Instance.EndSubsegment();
            }
        }

        public static BudgetDetail GetS3Budget(string userId)
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            AWSXRayRecorder.Instance.BeginSubsegment("MyBudgetExplorer.Models.Cache.GetS3Budget()");
            try
            {
                if (string.IsNullOrWhiteSpace(awsAccessKey) || string.IsNullOrWhiteSpace(awsSecretKey))
                    return null;

                var binaryFormatter = new BinaryFormatter();
                var aesKey = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(userId));
                var aesIV = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(userId));
                var hash = BitConverter.ToString(aesKey).Replace("-", "").ToLower();
                var fileName = $"budget.{hash}";
                var tempFilePath = Path.Combine(Path.GetTempPath(), fileName);

                BudgetDetail budget = null;
                using (IAmazonS3 client = new AmazonS3Client(awsAccessKey, awsSecretKey, RegionEndpoint.USEast2))
                {
                    byte[] encrypted = null;
                    GetObjectRequest request = new GetObjectRequest
                    {
                        BucketName = bucketName,
                        Key = fileName
                    };
                    try
                    {
                        using (GetObjectResponse response = client.GetObjectAsync(request).Result)
                        {
                            byte[] buffer = new byte[128 * 1024];
                            using (MemoryStream ms = new MemoryStream())
                            {
                                int read;
                                while ((read = response.ResponseStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    ms.Write(buffer, 0, read);
                                }
                                encrypted = ms.ToArray();
                            }
                        }

                        var decrypted = DecryptAES(encrypted, aesKey, aesIV);
                        using (var ms = new MemoryStream(decrypted))
                        {
                            budget = (BudgetDetail)binaryFormatter.Deserialize(ms);
                        }

                        if (encrypted != null && encrypted.Length > 0)
                        {
                            File.WriteAllBytes(tempFilePath, encrypted);
                            File.SetCreationTimeUtc(tempFilePath, budget.LastModifiedOn);
                            File.SetLastWriteTimeUtc(tempFilePath, budget.LastModifiedOn);
                        }
                    }
                    catch
                    {
                    }

                    return budget;
                }
            }
            finally
            {
                AWSXRayRecorder.Instance.EndSubsegment();
            }
        }

        public static BudgetDetail GetApiBudget(string accessToken, string userId)
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            AWSXRayRecorder.Instance.BeginSubsegment("MyBudgetExplorer.Models.Cache.GetApiBudget()");
            try
            {
                var binaryFormatter = new BinaryFormatter();
                var aesKey = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(userId));
                var aesIV = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(userId));
                var hash = BitConverter.ToString(aesKey).Replace("-", "").ToLower();
                var fileName = $"budget.{hash}";
                var tempFilePath = Path.Combine(Path.GetTempPath(), fileName);

                BudgetDetail budget = null;
                byte[] encrypted = null;

                var api = new YnabApi(accessToken);
                var tempBudget = api.GetBudget();
                budget = tempBudget.Data.Budget;
                budget.LastModifiedOn = DateTime.UtcNow;

                using (var ms = new MemoryStream())
                {
                    binaryFormatter.Serialize(ms, budget);
                    encrypted = EncryptAES(ms.ToArray(), aesKey, aesIV);
                }

                // Store S3 File
                if (!string.IsNullOrWhiteSpace(awsAccessKey) && !string.IsNullOrWhiteSpace(awsSecretKey))
                    using (IAmazonS3 client = new AmazonS3Client(awsAccessKey, awsSecretKey, RegionEndpoint.USEast2))
                    {
                        using (var ms = new MemoryStream(encrypted))
                        {
                            var putrequest = new PutObjectRequest
                            {
                                BucketName = bucketName,
                                Key = fileName,
                                InputStream = ms
                            };

                            client.PutObjectAsync(putrequest).Wait();
                        }
                    }

                // Store Local File
                if (encrypted != null && encrypted.Length > 0)
                {
                    File.WriteAllBytes(tempFilePath, encrypted);
                    File.SetCreationTimeUtc(tempFilePath, budget.LastModifiedOn);
                    File.SetLastWriteTimeUtc(tempFilePath, budget.LastModifiedOn);
                }

                return budget;
            }
            finally
            {
                AWSXRayRecorder.Instance.EndSubsegment();
            }
        }

        public static BudgetDetail GetBudget(string accessToken, string userId)
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            AWSXRayRecorder.Instance.BeginSubsegment("MyBudgetExplorer.Models.Cache.GetBudget()");
            try
            {
                // Local File
                BudgetDetail budget = GetLocalBudget(userId);
                if (budget != null)
                    return budget;

                // S3 File
                budget = GetS3Budget(userId);
                if (budget != null)
                    return budget;

                // API File
                budget = GetApiBudget(accessToken, userId);

                return budget;
            }
            catch (Exception e)
            {
                AWSXRayRecorder.Instance.AddException(e);
                throw;
            }
            finally
            {
                AWSXRayRecorder.Instance.EndSubsegment();
            }
        }

        public static Forecast GetLocalForecast(string userId)
        {
            AWSXRayRecorder.Instance.BeginSubsegment("MyBudgetExplorer.Models.Cache.GetLocalForecast()");
            try
            {
                var binaryFormatter = new BinaryFormatter();
                var aesKey = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(userId));
                var aesIV = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(userId));
                var hash = BitConverter.ToString(aesKey).Replace("-", "").ToLower();
                var fileName = $"forecast.{hash}";
                var tempFilePath = Path.Combine(Path.GetTempPath(), fileName);
                var budgetFileName = $"budget.{hash}";
                var budgetFilePath = Path.Combine(Path.GetTempPath(), budgetFileName);

                if (!File.Exists(budgetFilePath))
                    return null;

                Forecast forecast = null;
                if (File.Exists(tempFilePath))
                {
                    if (File.GetCreationTimeUtc(budgetFilePath) > File.GetCreationTimeUtc(tempFilePath))
                        return null;

                    try
                    {
                        var decrypted = DecryptAES(File.ReadAllBytes(tempFilePath), aesKey, aesIV);
                        using (var ms = new MemoryStream(decrypted))
                        {
                            forecast = (Forecast)binaryFormatter.Deserialize(ms);
                        }
                    }
                    catch
                    {
                        AWSXRayRecorder.Instance.AddAnnotation("Forecast-Local-Cache", "File found, deserialize failed.");
                        File.Delete(tempFilePath);
                    }
                }

                return forecast;
            }
            finally
            {
                AWSXRayRecorder.Instance.EndSubsegment();
            }
        }

        public static Forecast GetForecast(string accessToken, string userId)
        {
            AWSXRayRecorder.Instance.BeginSubsegment("MyBudgetExplorer.Models.Cache.GetForecast()");
            try
            {
                var binaryFormatter = new BinaryFormatter();
                var aesKey = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(userId));
                var aesIV = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(userId));
                var hash = BitConverter.ToString(aesKey).Replace("-", "").ToLower();
                var fileName = $"forecast.{hash}";
                var tempFilePath = Path.Combine(Path.GetTempPath(), fileName);

                byte[] encrypted = null;

                // Local File
                Forecast forecast = GetLocalForecast(userId);
                if (forecast != null)
                    return forecast;

                // Create Forecast
                forecast = Forecast.Create(accessToken, userId);

                // Serialize & Encrypt Forecast
                using (var ms = new MemoryStream())
                {
                    binaryFormatter.Serialize(ms, forecast);
                    encrypted = EncryptAES(ms.ToArray(), aesKey, aesIV);
                }

                // Store Local File
                if (encrypted != null && encrypted.Length > 0)
                {
                    File.WriteAllBytes(tempFilePath, encrypted);
                    File.SetCreationTimeUtc(tempFilePath, forecast.LastModifiedOn);
                    File.SetLastWriteTimeUtc(tempFilePath, forecast.LastModifiedOn);
                }

                return forecast;
            }
            catch (Exception e)
            {
                AWSXRayRecorder.Instance.AddException(e);
                throw;
            }
            finally
            {
                AWSXRayRecorder.Instance.EndSubsegment();
            }
        }

    }
}
