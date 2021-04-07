using System.Collections;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public Renderer textureRenderer2;
    public SpriteRenderer backroundSR;

    public void DrowTexture2(Texture2D mainTexture, Texture2D miningTexture)
    {
        textureRenderer2.sharedMaterial.mainTexture = mainTexture;
        textureRenderer2.transform.localScale = new Vector3(mainTexture.width, 1, mainTexture.height);
        textureRenderer2.transform.position = new Vector2(mainTexture.width / 2f - 0.5f, mainTexture.height / 2f - 0.5f) * 10;
        textureRenderer2.sharedMaterial.SetTexture("MiningRegion", miningTexture);
    }

    public void DrowTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
        textureRenderer.transform.position = new Vector2(texture.width / 2f - 0.5f, texture.height / 2f - 0.5f) * 10;
    }

    public void SetBacground(Texture2D texture, int mapWidth, int mapHeight)
    {
        backroundSR.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0f, 0f));
        float scaleX = mapWidth  * 1000 / texture.width;
        float scaleY = mapHeight * 1000 / texture.height;
        backroundSR.transform.localScale = new Vector2(scaleX, scaleY);
    }
}
