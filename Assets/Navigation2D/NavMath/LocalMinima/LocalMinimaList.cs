using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Navigation2D.NavMath.LocalMinima
{
    public static class LocalMinimaList
    {
        public static List<Tuple<List<LineSegment2D>, List<LineSegment2D>>> GetLocalMinimaList(Shape2D shape)
        {
            var points = shape.Points;
            List<LocalMinimaLineSegment2D> segments = new();
            Vector2 min = points[0];
            for(int i = 1; i < points.Count; i++)
            {
                if (points[i].y < min.y) 
                    min = points[i];
                else if (points[i].y == min.y)
                    min = points[i].x < min.x ? points[i] : min;
            }
            Vector2 previous = min;
            var index = points.IndexOf(min);
            bool isLeft = true;
            int iter = index;
            do
            {
                var iter2 = iter + 1 < points.Count ? iter + 1 : 0;
                if (previous.y > points[iter2].y && isLeft)
                    isLeft = false;
                else if (previous.y < points[iter2].y && !isLeft)
                    isLeft = true;

                segments.Add(
                    new LocalMinimaLineSegment2D(
                            new(points[iter].x, points[iter].y),
                            new(points[iter2].x, points[iter2].y),
                            PointSortingMode.IncreasingYX)
                        {IsLeftToInterior = isLeft});

                Debug.DrawLine(segments[^1].P1, segments[^1].P2, segments[^1].IsLeftToInterior? Color.red : Color.blue, 5f);
                
                previous = points[iter2];
                if (iter == points.Count - 1)
                    iter = 0;
                else
                    iter++;
            } while (iter != index);
            
            var events = new PriorityQueue<Event, Event>();
            
            foreach (var segment in segments)
            {
                var ev = new Event(segment, EventType.SegmentStart, PointSortingMode.IncreasingYX);
                events.Enqueue(ev, ev);
            }
            
            List<Tuple<List<LineSegment2D>, List<LineSegment2D>>> list = new();
            
            while (events.Count > 0)
            {
                var currentEvent = events.Dequeue();
                
                var found = false;
                foreach (var l in list)
                {
                    if (((LocalMinimaLineSegment2D) currentEvent.Segment).IsLeftToInterior)
                    {
                        if (l.Item1.Count > 0 && l.Item1[^1].P2 == currentEvent.Point ||
                            l.Item2.Count > 0 && l.Item2[0].P1 == currentEvent.Point)
                        {
                            l.Item1.Add(currentEvent.Segment);
                            found = true;
                            break;
                        }
                    }
                    else
                    {
                        if (l.Item2.Count > 0 && l.Item2[^1].P2 == currentEvent.Point ||
                            l.Item1.Count > 0 && l.Item1[0].P1 == currentEvent.Point)
                        {
                            l.Item2.Add(currentEvent.Segment);
                            found = true;
                            break;
                        }
                    }
                }
                
                if (!found)
                {
                    list.Add(new (new (), new ()));

                    if (((LocalMinimaLineSegment2D) currentEvent.Segment).IsLeftToInterior)
                    {
                        list[^1].Item1.Add(currentEvent.Segment);
                    }
                    else
                    {
                        list[^1].Item2.Add(currentEvent.Segment);
                    }
                }
            }

            return list;
        }
    }
}