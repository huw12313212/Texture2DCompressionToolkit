using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureCompressionToolkit : MonoBehaviour
{

    [MenuItem("Assets/RCGs/TextureCompressionToolkit/Sprite2D-Resize-MultipluOf4")]
    static void Sprite2DResize()
    {
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            Object obj = Selection.objects[i];
            var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite == null)
            {
                Debug.LogError("It's not a Sprite: " + path);
                continue;
            }
            else if (!path.ToLower().EndsWith(".png") &&
            !path.ToLower().EndsWith(".jpg") &&
            !path.ToLower().EndsWith(".jpeg") &&
            !path.ToLower().EndsWith(".tga") &&
            !path.ToLower().EndsWith(".exr"))
            {
                Debug.LogError("It's not a PNG/JPG/JPEG/TGA/EXR file: " + path);
                continue;
            }
            else
            {
                ResizeToMultipleOf4(sprite, path);
            }
        }

        AssetDatabase.Refresh();
    }


    public static void ResizeToMultipleOf4(Sprite sprite, string path)
    {
        int originalWidth = sprite.texture.width;
        int originalHeight = sprite.texture.height;
        int targetWidth = FindNextMultipleOf4(originalWidth);
        int targetHeight = FindNextMultipleOf4(originalHeight);


        if (originalWidth == targetWidth && originalHeight == targetHeight)
        {
            Debug.Log(path + " is already MultipleOf4");
            return;
        }

        int fullXPadding = (targetWidth - originalWidth);
        int fullYPadding = (targetHeight - originalHeight);
        // int halfXPadding = Mathf.FloorToInt(fullXPadding / 2.0f);
        //int halfYPadding = Mathf.FloorToInt(fullYPadding / 2.0f);

        Debug.Log(sprite.pivot);

        int offsetX = Mathf.FloorToInt(fullXPadding * (sprite.pivot.x / (float)sprite.texture.width));
        int offsetY = Mathf.FloorToInt(fullYPadding * (sprite.pivot.y / (float)sprite.texture.height));




        Texture2D originalTexture = new Texture2D(originalWidth, originalHeight);
        originalTexture.LoadImage(File.ReadAllBytes(path));

        Texture2D texture = new Texture2D(targetWidth, targetHeight);

        for (int x = 0; x < targetWidth; x++)
        {
            for (int y = 0; y < targetHeight; y++)
            {
                int innerX = x - offsetX;
                int innerY = y - offsetY;

                if (innerX < 0 || innerX >= originalWidth)
                {
                    texture.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
                else if (innerY < 0 || innerY >= originalHeight)
                {
                    texture.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
                else
                {
                    texture.SetPixel(x, y, originalTexture.GetPixel(innerX, innerY));
                }
            }
        }

        byte[] bytes = null;

        if (path.ToLower().EndsWith(".png"))
        {
            bytes = texture.EncodeToPNG();
        }
        else if (path.ToLower().EndsWith(".jpg"))
        {
            bytes = texture.EncodeToJPG();
        }
        else if (path.ToLower().EndsWith(".jpeg"))
        {
            bytes = texture.EncodeToJPG();
        }
        else if (path.ToLower().EndsWith(".tga"))
        {
            bytes = texture.EncodeToTGA();
        }
        else if (path.ToLower().EndsWith(".exr"))
        {
            bytes = texture.EncodeToEXR();
        }


       // texture.EncodeToEXR();

        File.Delete(path);
        File.WriteAllBytes(path, bytes);

        DestroyImmediate(texture);
        DestroyImmediate(originalTexture);
        Debug.Log(path + " From (" + originalWidth + ":" + originalHeight + ")" + " -> To (" + targetWidth + ":" + targetHeight + ")");
    }

    public static int FindNextMultipleOf4(int value)
    {
        float ratio = value / 4.0f;
        return Mathf.CeilToInt(ratio) * 4;
    }

    [MenuItem("Assets/RCGs/TextureCompressionToolkit/Sprite2D-Resize", true)]
    private static bool Sprite2DResizeValidate()
    {
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            if (Selection.objects[i].GetType() != typeof(Texture2D))
                return false;
        }

        return true;
    }




}
