using System.Collections.Generic;

namespace pizzaSolution
{
    internal class ShapeComparer : IComparer<Shape>
    {
        public int Compare(Shape first, Shape second)
        {
            var firstPlot = first.Height * first.Width;
            var secondPlot = second.Height * second.Width;

            return secondPlot.CompareTo (firstPlot);
        }
    }
}