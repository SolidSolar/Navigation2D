using System.Collections.Generic;
using UnityEngine;

namespace Navigation2D.NavMath.PolygonSelfIntersectionCheck
{
    public class PolygonSelfIntersectionCheck
    {
        private PriorityQueue<Event, Event> _events;
        private List<PolygonLineSegment2D> _sweepline;

        public static bool HasIntersections(List<Vector2> points)
        {
            List<PolygonLineSegment2D> segments = new();
            for (int i = 0; i < points.Count-1; i++)
            {
                segments.Add(new(new(points[i].x, points[i].y), new(points[i+1].x, points[i+1].y), PointSortingMode.IncreasingXY));
            }
            segments.Add(new(new(points[^1].x, points[^1].y), new(points[0].x, points[0].y), PointSortingMode.IncreasingXY));
            
            var sweepline = new SortedList<LineSegment2D, LineSegment2D>();
            var events = new PriorityQueue<Event, Event>();
            
            foreach (var segment in segments)
            {
                var ev = new Event(segment, EventType.SegmentStart, PointSortingMode.IncreasingXY);
                var ev2 = new Event(segment, EventType.SegmentEnd, PointSortingMode.IncreasingXY);
                events.Enqueue(ev, ev);
                events.Enqueue(ev2, ev2);
            }
            
            while (events.Count > 0)
            {
                var currentEvent = events.Dequeue();
                switch (currentEvent.Type)
                {
                    case EventType.SegmentStart:
                        sweepline.Add(currentEvent.Segment, currentEvent.Segment);
                        int i = sweepline.IndexOfValue(currentEvent.Segment);
                        if (i > 0 && ((PolygonLineSegment2D)currentEvent.Segment).TryIntersect(((PolygonLineSegment2D)sweepline.Values[i-1]), out Vector2 v1, false))
                        {
                            return true;
                        }
                        if (i < sweepline.Count - 1 &&  ((PolygonLineSegment2D)currentEvent.Segment).TryIntersect(((PolygonLineSegment2D)sweepline.Values[i + 1]),  out Vector2 v2, false))
                        {
                            return true;
                        }
                        break;
                    case EventType.SegmentEnd:
                        int j = sweepline.IndexOfValue(currentEvent.Segment);
                        if (j > 0 && j < sweepline.Count - 1 &&  ((PolygonLineSegment2D)sweepline.Values[j - 1]).TryIntersect(((PolygonLineSegment2D)sweepline.Values[j + 1]),  out Vector2 v3, false))
                        {
                            return true;
                        }
                        sweepline.Remove(currentEvent.Segment);
                        break;
                }
            }
            return false;
        }

    }
}
