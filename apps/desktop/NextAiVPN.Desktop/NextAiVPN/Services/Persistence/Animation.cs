using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NextAiVPN.Services.Persistence;

public static class Animation
{
	private static readonly DoubleAnimation _doubleAnimation = new DoubleAnimation();

	private static readonly RotateTransform _rotateTransform = new RotateTransform();

	public static void SpinnerAnimation(bool animate, Image animationImage)
	{
		_doubleAnimation.From = 0.0;
		_doubleAnimation.To = 360.0;
		_doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1L));
		_doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
		animationImage.RenderTransform = _rotateTransform;
		animationImage.RenderTransformOrigin = new Point(0.5, 0.5);
		if (animate)
		{
			_rotateTransform.BeginAnimation(RotateTransform.AngleProperty, _doubleAnimation);
		}
		else
		{
			_rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
		}
	}
}
