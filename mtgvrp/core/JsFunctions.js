function getSafeResolution () {
	var offsetX = 0;
	var screen = API.getScreenResolutionMantainRatio();
	var screenX = screen.Width;
	var screenY = screen.Height;
	if (screenX / screenY > 1.7777) {
		// aspect ratio is larger than 16:9
		var idealBox = Math.ceil(screenY * 1.7777);
		// ex: 2850 - 1920 == 660 / 2 == 330
		offsetX = (screenX - idealBox) / 2;
		// and gotta set the ideal box to make it work
		screenX = idealBox;
	}

	return { Offset: offsetX, X: screenX, Y: screenY }
}

function scaleCoordsToReal (point) {
	var ratioScreen = getSafeResolution();
	var realScreen = API.getScreenResolution();

	var widthDivisor = realScreen.Width / ratioScreen.X;
	var heightDivisor = realScreen.Height / ratioScreen.Y;

	return { X: (point.X * widthDivisor) + ratioScreen.Offset, Y: point.Y * heightDivisor }
}