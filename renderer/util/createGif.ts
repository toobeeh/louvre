// source: https://raw.githubusercontent.com/toobeeh/skribbltypo/refs/heads/main/src/util/gif/createGif.ts
// slightly modified to use watermark colors

import { Color } from "./color";
import type { CanvasCommandProcessor } from "./canvasCommandProcessor";
import { GifEncoder } from "./gifEncoder";
import {addWatermark} from "./addWatermark";
import {Canvas} from "canvas";

/**
 * Create a gif from skribbl commands
 * @param processor
 * @param commands
 * @param commandResolution
 * @param frameDelay
 * @param frameCount
 * @param onFrameRendered
 */
export function createGif(processor: CanvasCommandProcessor, commands: number[][], commandResolution: number, frameDelay: number, frameCount: number, onFrameRendered?: (currentIndex: number, totalIndex: number) => void, watermark?: Canvas){

    const skribblColorCodes = new Set(commands.map(c => c[1]));
    if(skribblColorCodes.size > 256){
        throw new Error("Too many colors in the skribbl commands to render gif");
    }

    // add colors from watermark to palette
    if(watermark){
        const watermarkData = watermark.getContext("2d")?.getImageData(0, 0, watermark.width, watermark.height).data;
        if(watermarkData === undefined){
            throw new Error("Failed to get image data from watermark");
        }

        Array.from(watermarkData)
            .filter((v, i) => i % 4 === 0)
            .map((v, i) => Array.from(watermarkData.slice(i * 4, i * 4 + 3)))
            .forEach(c => skribblColorCodes.add(Color.fromRgb(c[0], c[1], c[2], c[3]).typoCode));
    }

    const colorSet = new Set([...skribblColorCodes.values()].map(c => Color.fromSkribblCode(c)));

    const gifEncoder = new GifEncoder(colorSet, frameCount);

    for(let i = 0; i < commands.length; i++){
        processor.processDrawCommand(commands[i]);
        if(i % commandResolution === 0) {
            let image = processor.exportImage();

            if(watermark != undefined){
                image = addWatermark(image, processor.width, processor.height, watermark)
            }

            gifEncoder.addFrame(image, frameDelay);
            onFrameRendered?.(i, commands.length);
        }
    }

    let finalFrame = processor.exportImage();
    if(watermark != undefined){
        finalFrame = addWatermark(finalFrame, processor.width, processor.height, watermark);
    }
    gifEncoder.addFrame(finalFrame, 2000);

    return gifEncoder.finalize();
}