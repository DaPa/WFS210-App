﻿using System;
using MonoTouch.CoreAnimation;
using System.Drawing;
using MonoTouch.UIKit;

namespace WFS210.UI
{
	public class XMarker : Marker
	{
		public CALayer Layer{ get; set;}
		PointF position;
		public XMarker (string resource,int pos) : base(resource)
		{
			position = new PointF(pos , Image.CGImage.Height /2 - 18);
			Layer = new CALayer ();
			Layer.Bounds = new RectangleF (0, 0, Image.CGImage.Width, Image.CGImage.Height);
			Layer.Position = position;
			Layer.Contents = Image.CGImage;
		}

		public int X
		{
			set
			{
				base.VariablePosition = value;
				position.X = base.VariablePosition;
			}
		}
	}
}

