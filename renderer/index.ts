import {parse} from "ts-command-line-args";
import {IRendererArguments} from "./renderer-arguments.interface";
import * as fs from "node:fs";
import {CanvasCommandProcessor} from "./util/canvasCommandProcessor";
import {createGif} from "./util/createGif";
import {createCanvas} from "canvas";

const args = parse<IRendererArguments>({
    commandsSkdPath: {type: String, alias: "c", description: "Path to the skd commands file"},
    gifOutputPath: {type: String, alias: "o", description: "Path where the rendered gif should be saved"},
    gifDuration: {type: Number, alias: "d", description: "Duration of the gif in milliseconds", defaultValue: 5000},
    gifFramerate: {type: Number, alias: "f", description: "Framerate of the gif in frames per second", defaultValue: 5}
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

// create gif
const gif = createGif(processor, commands, commandResolution, frameDelay, frameCount, (frameIndex, totalFrames) => console.log(`Rendered command ${frameIndex + 1} of ${totalFrames}`));

// save gif
gif.arrayBuffer().then(buffer => {
    fs.writeFileSync(args.gifOutputPath, Buffer.from(buffer));
    console.log(`GIF saved to ${args.gifOutputPath}`);
}).catch(e => {
    console.error("Failed to save gif:", e);
    process.exit(1);
});