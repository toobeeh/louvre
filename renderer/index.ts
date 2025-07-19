import {parse} from "ts-command-line-args";
import {IRendererArguments} from "./renderer-arguments.interface";
import * as fs from "node:fs";
import {CanvasCommandProcessor} from "./util/canvasCommandProcessor";
import {createGif} from "./util/createGif";
import {Canvas, createCanvas, loadImage} from "canvas";

const args = parse<IRendererArguments>({
    commandsSkdPath: {type: String, alias: "c", description: "Path to the skd commands file"},
    gifOutputPath: {type: String, alias: "o", description: "Path where the rendered gif should be saved"},
    gifDuration: {type: Number, alias: "d", description: "Duration of the gif in milliseconds", defaultValue: 5000},
    gifFramerate: {type: Number, alias: "f", description: "Framerate of the gif in frames per second", defaultValue: 5},
    watermarkPath: {type: String, alias: "w", description: "Optional path to a watermark image", optional: true}
});

// read input commands
let commands: number[][];
try {
    commands = JSON.parse(fs.readFileSync(args.commandsSkdPath, "utf8")) as number[][];
}
catch (e) {
    console.error("Failed to read commands from file:", e);
    process.exit(1);
}

// init renderer
const offscreenCanvas = createCanvas(800, 600);
const context = offscreenCanvas.getContext("2d");
if(!context) {
    console.error("Failed to get 2d context");
    process.exit(1);
}
const processor = new CanvasCommandProcessor(context);

// init gif parameters
const frameDelay = 1/args.gifFramerate * 1000; // in milliseconds
const frameCount = args.gifDuration / frameDelay; // total number of frames to render
const commandResolution = Math.max(1,Math.floor(commands.length / frameCount));

args.watermarkPath = "C:\\Users\\tobeh\\Desktop\\typo\\icons\\64MaxFit.png";

/* run async  */
(async () => {

    // load watermark
    let watermark: Canvas | undefined = undefined;
    if(args.watermarkPath) {
        try {
            const watermarkImage = await loadImage(args.watermarkPath);
            watermark = createCanvas(watermarkImage.width, watermarkImage.height);
            const wmCtx = watermark.getContext("2d");
            wmCtx.drawImage(watermarkImage, 0, 0);
        } catch (e) {
            console.error("Failed to load watermark image:", e);
            process.exit(1);
        }
    }

    // create gif
    const gif = createGif(processor, commands, commandResolution, frameDelay, frameCount, (frameIndex, totalFrames) => console.log(`Rendered command ${frameIndex + 1} of ${totalFrames}`), watermark);

    // save gif
    const buffer = await gif.arrayBuffer();
    fs.writeFileSync(args.gifOutputPath, Buffer.from(buffer));

})().then(() => {
    console.log(`GIF saved to ${args.gifOutputPath}`);
}).catch(e => {
    console.error("Failed to save gif:", e);
    process.exit(1);
});