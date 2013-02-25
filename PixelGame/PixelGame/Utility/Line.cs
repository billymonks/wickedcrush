using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WickedCrush
{
    public class Line // line segments, to be used as sensors
    {
        public Point start;
        public Point end;

        public Line(Point start, Point end) // perhaps updated with ref ? micro-optimization
        {
            this.start = start;
            this.end = end;
        }

        //public Line(int a, int b, int c, int d)
        //{
            //start = new Point(a, b);
            //end = new Point(c, d);
        //}

        public bool collision(Rectangle r) // probably bloated
        {
            Line top = new Line(new Point(r.X, r.Y), new Point(r.X + r.Width, r.Y));
            Line bottom = new Line(new Point(r.X, r.Y + r.Height), new Point(r.X + r.Width, r.Y + r.Height));
            Line left = new Line(new Point(r.X, r.Y), new Point(r.X, r.Y + r.Height));
            Line right = new Line(new Point(r.X + r.Width, r.Y), new Point(r.X + r.Width, r.Y + r.Height));
            return intersects(top) || intersects(bottom) || intersects(left) || intersects(right) || r.Contains(start.X, start.Y) || r.Contains(end.X, end.Y);
        }

        public bool ccw(Point a, Point b, Point c)
        {
            return (c.Y - a.Y) * (b.X - a.X) > (b.Y - a.Y) * (c.X - a.X);
        }

        public bool intersects(Line l)
        {
            return ccw(this.start,l.start,l.end) != ccw(this.end,l.start, l.end) && ccw(this.start, this.end, l.start) != ccw(this.start, this.end, l.end);
        }
    }
}
