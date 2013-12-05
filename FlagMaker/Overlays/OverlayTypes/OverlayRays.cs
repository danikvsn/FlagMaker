using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FlagMaker.Overlays.OverlayTypes
{
	internal class OverlayRays : Overlay
	{
		public OverlayRays(int maximumX, int maximumY)
			: base(new List<Attribute>
			       {
				       new Attribute("X", true, 1, true),
				       new Attribute("Y", true, 1, false),
				       new Attribute("Count", true, 4, true)
			       }, maximumX, maximumY)
		{
		}

		public OverlayRays(Color color, int x, int y, int count, int maximumX, int maximumY)
			: base(color, new List<Attribute>
			              {
				              new Attribute("X", true, x, true),
				              new Attribute("Y", true, y, false),
				              new Attribute("Count", true, count, true)
			              }, maximumX, maximumY)
		{
		}

		public override string Name { get { return "rays"; } }

		public override void Draw(Canvas canvas)
		{
			foreach (var path in GetPaths(canvas.Width, canvas.Height))
			{
				canvas.Children.Add(new Path
									{
										Fill = new SolidColorBrush(Color),
										Data = Geometry.Parse(path),
										SnapsToDevicePixels = true
									});
			}
		}

		public override string ExportSvg(int width, int height)
		{
			var sb = new StringBuilder();

			foreach (var path in GetPaths(width, height))
			{
				sb.Append(string.Format("<path d=\"{0}\" fill=\"#{1}\" />",
					path, Color.ToHexString()));
			}

			return sb.ToString();
		}

		private IEnumerable<string> GetPaths(double width, double height)
		{
			var centerX = width * (Attributes.Get("X").Value / MaximumX);
			var centerY = height * (Attributes.Get("Y").Value / MaximumY);
			var count = (int)Attributes.Get("Count").Value;
			double angularInterval = Math.PI / count;

			for (int i = 0; i < count; i++)
			{
				var point1 = BorderIntersection(centerX, centerY, angularInterval * 2 * i, width, height);
				var point2 = BorderIntersection(centerX, centerY, angularInterval * (2 * i + 1), width, height);

				// If points lie on different sides, add corner
				string point3 = string.Empty;
				if (point1.X != point2.X && point1.Y != point2.Y)
				{
					if (point1.Y == 0)
					{
						point3 = "0,0 ";
					}
					else if (point1.X == 0)
					{
						point3 = string.Format("0,{0} ", height);
					}
					else if (point1.Y == height)
					{
						point3 = string.Format("{0},{1} ", width, height);
					}
					else if (point1.X == width)
					{
						point3 = string.Format("{0},0 ", width);
					}
				}

				yield return string.Format("M {0},{1} {2},{3} {4}{5},{6} Z",
					centerX, centerY,
					point1.X, point1.Y,
					point3,
					point2.X, point2.Y);
			}
		}

		private Point BorderIntersection(double centerX, double centerY, double angle, double width, double height)
		{
			var possiblePoints = new List<Point>();

			if (angle > 0 && angle < Math.PI)
			{
				// Check intersection with top border
				var tX = centerY / Math.Tan(angle);
				possiblePoints.Add(new Point(centerX + tX, 0));
			}
			if (angle > Math.PI / 2 && angle < 3 * Math.PI / 2)
			{
				// Check intersection with left border
				var tY = centerX * Math.Tan(2 * Math.PI - angle);
				possiblePoints.Add(new Point(0, centerY - tY));
			}
			if (angle > Math.PI && angle < 2 * Math.PI)
			{
				// Check intersection with bottom border
				var tX = Math.Tan(3 * Math.PI / 2 - angle) * (height - centerY);
				possiblePoints.Add(new Point(centerX - tX, height));
			}
			if (angle < Math.PI / 2)
			{
				// Check intersection with right border
				var tY = Math.Tan(angle) * (width - centerX);
				possiblePoints.Add(new Point(width, centerY - tY));
			}
			if (angle > 3 * Math.PI / 2)
			{
				// Check intersection with right border
				var tY = Math.Tan(2 * Math.PI - angle) * (width - centerX);
				possiblePoints.Add(new Point(width, centerY + tY));
			}

			possiblePoints = possiblePoints.OrderBy(p => Length(p, new Point(centerX, centerY))).ToList();

			return possiblePoints.Any()
				? possiblePoints.First()
				: new Point(centerX, centerY);
		}

		private double Length(Point p1, Point p2)
		{
			return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
		}

		public override void SetValues(List<double> values)
		{
			Attributes.Get("X").Value = values[0];
			Attributes.Get("Y").Value = values[1];
			Attributes.Get("Count").Value = values[2];
		}

		public override IEnumerable<Shape> Thumbnail
		{
			get
			{
				return new List<Shape>
				       {
					       new Path
						   {
							   Data = Geometry.Parse(
								"M 15,10 18,0 12,0 Z M 15,10 0,8 0,12 Z M 15,10 18,20 12,20 Z M 15,10 30,8 30,12 Z" +
								"M 15,10 6,0 0,0 0,3 Z M 15,10 24,0 30,0 30,3 Z M 15,10 24,20 30,20 30,17 Z M 15,10 6,20 0,20 0,17 Z"),
						       SnapsToDevicePixels = true
						   }
				       };
			}
		}
	}
}