using UnityEngine;
using UnityEngine.UI;
using SFramework.Core.Module.Encryption;

public class EncryptionDemo : MonoBehaviour
{
    [Header("UI 组件")]
    public InputField inputText;
    public InputField passwordField;
    public Text outputText;
    public Button encryptButton;
    public Button decryptButton;
    public Button generateKeysButton;
    
    [Header("显示区域")]
    public Text logText;
    
    private string encryptedData;
    private string publicKey;
    private string privateKey;
    
    void Start()
    {
        // 绑定按钮事件
        encryptButton.onClick.AddListener(OnEncryptClicked);
        decryptButton.onClick.AddListener(OnDecryptClicked);
        generateKeysButton.onClick.AddListener(OnGenerateKeysClicked);
        
        // 初始化
        LogMessage("Song Encryption Library 已加载");
        
        // 测试库是否正常工作
        TestEncryption();
    }
    
    void OnEncryptClicked()
    {
        string data = inputText.text;
        string password = passwordField.text;
        
        if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(password))
        {
            LogMessage("请输入数据和密码");
            return;
        }
        
        // AES 加密
        encryptedData = SongEncryption.AESEncrypt(data, password);
        
        if (encryptedData != null)
        {
            outputText.text = encryptedData;
            LogMessage($"AES 加密成功: {encryptedData.Substring(0, Mathf.Min(50, encryptedData.Length))}...");
        }
        else
        {
            LogMessage("AES 加密失败");
        }
    }
    
    void OnDecryptClicked()
    {
        string password = passwordField.text;
        
        if (string.IsNullOrEmpty(encryptedData) || string.IsNullOrEmpty(password))
        {
            LogMessage("请先加密数据并输入密码");
            return;
        }
        
        // AES 解密
        string decryptedData = SongEncryption.AESDecrypt(encryptedData, password);
        
        if (decryptedData != null)
        {
            outputText.text = decryptedData;
            LogMessage($"AES 解密成功: {decryptedData}");
        }
        else
        {
            LogMessage("AES 解密失败");
        }
    }
    
    void OnGenerateKeysClicked()
    {
        LogMessage("正在生成 RSA 密钥对...");
        
        var keyPair = SongEncryption.GenerateRSAKeyPair();
        
        if (keyPair.publicKey != null && keyPair.privateKey != null)
        {
            publicKey = keyPair.publicKey;
            privateKey = keyPair.privateKey;
            
            LogMessage($"RSA 密钥生成成功");
            LogMessage($"公钥长度: {publicKey.Length} 字符");
            LogMessage($"私钥长度: {privateKey.Length} 字符");
        }
        else
        {
            LogMessage("RSA 密钥生成失败");
        }
    }
    
    void TestEncryption()
    {
        LogMessage("=== 开始加密库测试 ===");
        
        string testData = "Hello Unity! 这是一个测试消息。";
        string testPassword = "UnityTestPassword123!";
        
        // 测试 AES 加密/解密
        string encrypted = SongEncryption.AESEncrypt(testData, testPassword);
        if (encrypted != null)
        {
            LogMessage("✓ AES 加密测试通过");
            
            string decrypted = SongEncryption.AESDecrypt(encrypted, testPassword);
            if (decrypted == testData)
            {
                LogMessage("✓ AES 解密测试通过");
            }
            else
            {
                LogMessage("✗ AES 解密测试失败");
            }
        }
        else
        {
            LogMessage("✗ AES 加密测试失败");
        }
        
        // 测试 ChaCha20 加密
        string chachaEncrypted = SongEncryption.ChaCha20Encrypt(testData, testPassword);
        if (chachaEncrypted != null)
        {
            LogMessage("✓ ChaCha20 加密测试通过");
        }
        else
        {
            LogMessage("✗ ChaCha20 加密测试失败");
        }
        
        // 测试密钥派生
        string derivedKey = SongEncryption.DeriveKey(testPassword, "test_salt", 1000);
        if (derivedKey != null)
        {
            LogMessage("✓ 密钥派生测试通过");
        }
        else
        {
            LogMessage("✗ 密钥派生测试失败");
        }
        
        LogMessage("=== 加密库测试完成 ===");
    }
    
    void LogMessage(string message)
    {
        Debug.Log(message);
        if (logText != null)
        {
            logText.text += message + "\n";
            
            // 限制日志长度
            if (logText.text.Length > 2000)
            {
                logText.text = logText.text.Substring(logText.text.Length - 1500);
            }
        }
    }
} 