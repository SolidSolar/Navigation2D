using System;
using UnityEngine;

namespace Navigation2D.NavMath
{
    public enum EventType{ SegmentStart, SegmentEnd };

    class Event : IComparable
    {
        private LineSegment2D _segment;
        private EventType _type;
        private Vector2 point;
        private PointSortingMode _sortingMode;

        /// <summary>
        /// The type of the event
        /// </summary>
        public EventType Type { get { return _type; } }

        /// <summary>
        /// The segment that this event pertains to
        /// </summary>
        public LineSegment2D Segment { get { return _segment; } }
        
        /// <summary>
        /// The point where the event occurs
        /// </summary>
        public Vector2 Point
        {
            get
            {
                switch (_type)
                {
                    case EventType.SegmentStart:
                        return _segment.P1;
                    case EventType.SegmentEnd:
                        return _segment.P2;
                    default:
                        throw new System.InvalidOperationException();
                }
            }
        }

        /// <summary>
        /// Creates a new event
        /// </summary>
        /// <param name="segment">the segement that this event pertains to</param>
        /// <param name="type">the type of the event</param>
        public Event(LineSegment2D segment, EventType type, PointSortingMode sortingMode)
        {
            _segment = segment;
            _type = type;
            _sortingMode = sortingMode;
        }
        
        public int CompareTo(object obj)
        {
            return NavMath.ComparePoints(Point, ((Event) obj).Point, _sortingMode);
        }
      
        public override string ToString()
        {
            return String.Format("Event: {0} {1} {2}", _type, Point, _segment);
        }
    }
}
