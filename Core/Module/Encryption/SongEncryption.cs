using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SFramework.Core.Module.Encryption
{
    /// <summary>
    /// 加密工具类
    /// </summary>
    public static class SongEncryption
    {
        // DLL 导入声明
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private const string DLL_NAME = "song_encryption";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        private const string DLL_NAME = "libsong_encryption";
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        private const string DLL_NAME = "libsong_encryption";
#else
        private const string DLL_NAME = "song_encryption";
#endif

        // C FFI 函数声明
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int aes_encrypt(
            [MarshalAs(UnmanagedType.LPStr)] string data,
            [MarshalAs(UnmanagedType.LPStr)] string password,
            out IntPtr result
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int aes_decrypt(
            [MarshalAs(UnmanagedType.LPStr)] string encryptedData,
            [MarshalAs(UnmanagedType.LPStr)] string password,
            out IntPtr result
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int rsa_generate_keypair(
            out IntPtr publicKey,
            out IntPtr privateKey
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int chacha20_encrypt(
            [MarshalAs(UnmanagedType.LPStr)] string data,
            [MarshalAs(UnmanagedType.LPStr)] string password,
            out IntPtr result
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int derive_key(
            [MarshalAs(UnmanagedType.LPStr)] string password,
            [MarshalAs(UnmanagedType.LPStr)] string salt,
            uint iterations,
            out IntPtr result
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void free_string(IntPtr str);

        // 辅助方法：将 IntPtr 转换为字符串并释放内存
        private static string PtrToStringAndFree(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return null;

            string result = Marshal.PtrToStringAnsi(ptr);
            free_string(ptr);
            return result;
        }

        // 公共 API 方法

        /// <summary>
        /// AES-256-GCM 加密
        /// </summary>
        /// <param name="data">要加密的数据</param>
        /// <param name="password">密码</param>
        /// <returns>加密后的 Base64 字符串</returns>
        public static string AESEncrypt(string data, string password)
        {
            IntPtr resultPtr;
            int errorCode = aes_encrypt(data, password, out resultPtr);

            if (errorCode != 0)
            {
                Debug.LogError($"AES 加密失败，错误代码: {errorCode}");
                return null;
            }

            return PtrToStringAndFree(resultPtr);
        }

        /// <summary>
        /// AES-256-GCM 解密
        /// </summary>
        /// <param name="encryptedData">加密的 Base64 字符串</param>
        /// <param name="password">密码</param>
        /// <returns>解密后的原始数据</returns>
        public static string AESDecrypt(string encryptedData, string password)
        {
            IntPtr resultPtr;
            int errorCode = aes_decrypt(encryptedData, password, out resultPtr);

            if (errorCode != 0)
            {
                Debug.LogError($"AES 解密失败，错误代码: {errorCode}");
                return null;
            }

            return PtrToStringAndFree(resultPtr);
        }

        /// <summary>
        /// ChaCha20Poly1305 加密
        /// </summary>
        /// <param name="data">要加密的数据</param>
        /// <param name="password">密码</param>
        /// <returns>加密后的 Base64 字符串</returns>
        public static string ChaCha20Encrypt(string data, string password)
        {
            IntPtr resultPtr;
            int errorCode = chacha20_encrypt(data, password, out resultPtr);

            if (errorCode != 0)
            {
                Debug.LogError($"ChaCha20 加密失败，错误代码: {errorCode}");
                return null;
            }

            return PtrToStringAndFree(resultPtr);
        }

        /// <summary>
        /// 生成 RSA 密钥对
        /// </summary>
        /// <returns>公钥和私钥的元组</returns>
        public static (string publicKey, string privateKey) GenerateRSAKeyPair()
        {
            IntPtr publicKeyPtr, privateKeyPtr;
            int errorCode = rsa_generate_keypair(out publicKeyPtr, out privateKeyPtr);

            if (errorCode != 0)
            {
                Debug.LogError($"RSA 密钥生成失败，错误代码: {errorCode}");
                return (null, null);
            }

            string publicKey = PtrToStringAndFree(publicKeyPtr);
            string privateKey = PtrToStringAndFree(privateKeyPtr);

            return (publicKey, privateKey);
        }

        /// <summary>
        /// PBKDF2 密钥派生
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="salt">盐值</param>
        /// <param name="iterations">迭代次数</param>
        /// <returns>派生的密钥 (Base64)</returns>
        public static string DeriveKey(string password, string salt, uint iterations = 10000)
        {
            IntPtr resultPtr;
            int errorCode = derive_key(password, salt, iterations, out resultPtr);

            if (errorCode != 0)
            {
                Debug.LogError($"密钥派生失败，错误代码: {errorCode}");
                return null;
            }

            return PtrToStringAndFree(resultPtr);
        }
    }
}