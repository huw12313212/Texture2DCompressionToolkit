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
        //Remove Duplicate Entry
        List<string> allPaths = new List<string>();
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            Object obj = Selection.objects[i];
            var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            if (!allPaths.Contains(path))
            {
                allPaths.Add(path);
            }
            else
            {
                // Debug.Log("[Ignore] Detect Duplicated Path: " + path);
            }
        }


        for (int i = 0; i < allPaths.Count; i++)
        {
            var path = allPaths[i];
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            Object[] data = AssetDatabase.LoadAllAssetsAtPath(path);




            if (sprite == null)
            {
                if (data.Length > 0 && data[0] is Sprite)
                {
                    sprite = data[0] as Sprite;
                }
            }

            if (sprite == null)
            {
                Debug.LogError("It's not a Sprite: " + path);
                continue;
            }
            else if (!path.ToLower().EndsWith(".png") &&
            !path.ToLower().EndsWith(".jpg") &&
            !path.ToLower().EndsWith(".jpeg") /*&&
           !path.ToLower().EndsWith(".tga") &&
            !path.ToLower().EndsWith(".exr")*/)
            {
                Debug.LogError("It's not a PNG/JPG/JPEG file: " + path, sprite);
                continue;
            }
            else
            {
                bool isMultiple = sprite.textureRect.width != sprite.texture.width || sprite.textureRect.height != sprite.texture.height;
                ResizeToMultipleOf4(sprite, path, isMultiple);
            }
        }
        AssetDatabase.Refresh();
    }


    public static void ResizeToMultipleOf4(Sprite sprite, string path, bool isMultiple)
    {
        Texture2D originalTexture = new Texture2D(1, 1);
        originalTexture.LoadImage(File.ReadAllBytes(path));
        int originalWidth = originalTexture.width;
        int originalHeight = originalTexture.height;
        int targetWidth = FindNextMultipleOf4(originalWidth);
        int targetHeight = FindNextMultipleOf4(originalHeight);


        if (sprite.texture.width != originalWidth || sprite.texture.height != originalHeight)
        {
            Debug.LogError("[Error] Please Remove Max Size : " + path, sprite);
        }
        else if (originalWidth == targetWidth && originalHeight == targetHeight)
        {
            // Debug.Log("[Ignore] " + path + " is already MultipleOf4");
            return;
        }

        int fullXPadding = (targetWidth - originalWidth);
        int fullYPadding = (targetHeight - originalHeight);

        int offsetX = 0;
        int offsetY = 0;

        if (isMultiple)
        {
            offsetX = 0;
            offsetY = 0;

        }
        else
        {
            offsetX = Mathf.FloorToInt(fullXPadding * (sprite.pivot.x / (float)sprite.texture.width));
            offsetY = Mathf.FloorToInt(fullYPadding * (sprite.pivot.y / (float)sprite.texture.height));
        }

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
        /* else if (path.ToLower().EndsWith(".tga"))
         {
             bytes = texture.EncodeToTGA();
         }
         else if (path.ToLower().EndsWith(".exr"))
         {
             bytes = texture.EncodeToEXR();
         }*/


        // texture.EncodeToEXR();

        File.Delete(path);
        File.WriteAllBytes(path, bytes);

        DestroyImmediate(texture);
        DestroyImmediate(originalTexture);
        Debug.Log(path + " From (" + originalWidth + ":" + originalHeight + ")" + " -> To (" + targetWidth + ":" + targetHeight + ")", sprite);
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
