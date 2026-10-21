using UnityEngine;

[System.Serializable]
public class BrandData
{
    public string brandName = "My Company";
    public Texture2D logoTexture; // загруженная текстура
    public Color primaryColor = Color.white;
    public Color secondaryColor = Color.black;
    public string logoFileName; // для сохранения
}