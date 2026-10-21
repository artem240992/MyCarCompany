using UnityEngine;
using System.IO;

public class BrandManager : MonoBehaviour
{
    public static BrandManager Instance { get; private set; }

    public Texture2D companyLogo { get; private set; }
    public string companyName { get; private set; } = "Моя Компания";
    
    // --- НОВЫЕ ПУТИ ---
    // Папка: Assets/StreamingAssets/IMG (в редакторе) или ..._Data/StreamingAssets/IMG (в билде)
    private string logoFolder => Path.Combine(Application.streamingAssetsPath, "IMG");
    private string logoPath => Path.Combine(logoFolder, "logo.png");
    
    private string nameKey = "CompanyNameKey";

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        // Если папки IMG нет, создаем её
        if (!Directory.Exists(logoFolder))
        {
            Directory.CreateDirectory(logoFolder);
            Debug.Log($"Создана папка для логотипов: {logoFolder}");
        }

        LoadLogoFromDisk();
        LoadCompanyName();
    }

    // === ЗАГРУЗКА ИЗ ВНЕШНЕГО ФАЙЛА (через проводник) ===
    public void LoadLogoFromFile(string sourceFilePath)
    {
        if (!File.Exists(sourceFilePath))
        {
            Debug.LogError($"Выбранный файл не найден: {sourceFilePath}");
            return;
        }

        try
        {
            // 1. Читаем выбранный файл
            byte[] fileData = File.ReadAllBytes(sourceFilePath);
            
            // 2. Создаем текстуру
            Texture2D newLogo = new Texture2D(2, 2);
            if (newLogo.LoadImage(fileData))
            {
                companyLogo = newLogo;
                
                // 3. СОХРАНЯЕМ файл в StreamingAssets/IMG/logo.png
                File.WriteAllBytes(logoPath, fileData);
                Debug.Log($"Логотип успешно сохранен в: {logoPath}");
                
                UIManager.Instance?.UpdateLogoUI(); 
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при сохранении логотипа: {e.Message}");
        }
    }

    // === ЗАГРУЗКА ИЗ ПАПКИ ПРИ СТАРТЕ ===
    private void LoadLogoFromDisk()
    {
        if (File.Exists(logoPath))
        {
            byte[] fileData = File.ReadAllBytes(logoPath);
            Texture2D loadedLogo = new Texture2D(2, 2);
            if (loadedLogo.LoadImage(fileData))
            {
                companyLogo = loadedLogo;
                Debug.Log($"Логотип загружен из: {logoPath}");
            }
        }
    }

    // === НАЗВАНИЕ КОМПАНИИ ===
    public void SetCompanyName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return;
        companyName = newName.Trim();
        PlayerPrefs.SetString(nameKey, companyName);
        PlayerPrefs.Save();
        UIManager.Instance?.UpdateCompanyNameUI();
    }

    private void LoadCompanyName()
    {
        companyName = PlayerPrefs.GetString(nameKey, "Моя Компания");
    }
}