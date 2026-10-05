namespace Wallpaper.Entities;

public sealed class Wallpaper
{
    /// <summary>壁纸日期（必应归档的 startdate）。</summary>
    public DateOnly Date { get; set; }

    /// <summary>版权描述文字（含 © 作者信息）。</summary>
    public required string Title { get; set; }

    /// <summary>预览/设壁纸用的大图地址（UHD 原图）。</summary>
    public required string ImageUrl { get; set; }

    /// <summary>列表缩略图地址。</summary>
    public required string ThumbnailUrl { get; set; }
}
