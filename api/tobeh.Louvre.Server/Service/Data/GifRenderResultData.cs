namespace tobeh.Louvre.Server.Service.Data;

public record GifRenderResultData(Ulid RenderId, byte[] GifContent, int Duration, int Fps, int Optimization);