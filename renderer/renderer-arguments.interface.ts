export interface IRendererArguments {
    commandsSkdPath: string; // path to the skd commands file
    gifOutputPath: string; // path where the rendered gif should be saved
    gifDuration: number; // duration of the gif in milliseconds
    gifFramerate: number; // framerate of the gif in frames per second
}