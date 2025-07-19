import {Canvas, createCanvas, ImageData} from 'canvas';

export function addWatermark(
    imageDataArray: Uint8ClampedArray,
    width: number,
    height: number,
    watermark: Canvas,
): Uint8ClampedArray {
    const margin = 10;

    // Create canvas and draw original image data
    const canvas = createCanvas(width, height);
    const ctx = canvas.getContext('2d');

    const imageData = new ImageData(imageDataArray, width, height);
    ctx.putImageData(imageData, 0, 0);

    // Draw watermark image in bottom-right
    ctx.drawImage(
        watermark,
        width - watermark.width - margin,
        height - watermark.height - margin,
        watermark.width,
        watermark.height
    );

    // Get updated image data
    const updatedImageData = ctx.getImageData(0, 0, width, height);
    return updatedImageData.data;
}
