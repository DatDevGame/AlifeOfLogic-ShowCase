using System.Collections.Generic;

public static class TextureDownloaderCacheManager
{
    private static Dictionary<string, TextureDownloader> textureDownloaderCache = new Dictionary<string, TextureDownloader>();
    public static TextureDownloader GetTextureDownloader(string url)
    {
        if(textureDownloaderCache.ContainsKey(url) == false)
            textureDownloaderCache.Add(url, new TextureDownloader(url));
        return textureDownloaderCache[url];
    }
}