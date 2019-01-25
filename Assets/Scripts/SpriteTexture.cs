using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteTexture : MonoBehaviour
{
    public Sprite sprite;

    private Renderer _renderer;
    public void Awake()
    {
        _renderer = GetComponent<Renderer>();
        Apply();
    }

    public void Apply()
    {
        _renderer.material.mainTexture = sprite.texture;
        Debug.Log(sprite.texture.width + " " + sprite.texture.height);
        Debug.Log(sprite.textureRect);

        _renderer.material.mainTextureScale = new Vector2(
            sprite.textureRect.width / sprite.texture.width,
            sprite.textureRect.height / sprite.texture.height
        );

        _renderer.material.mainTextureOffset = new Vector2(
            sprite.textureRect.x / sprite.texture.width,
            sprite.textureRect.y / sprite.texture.height
        );
    }
}
