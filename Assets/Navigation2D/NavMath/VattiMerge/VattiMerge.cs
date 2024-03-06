using System;
using System.Collections.Generic;
using System.Linq;
using Navigation2D.NavMath.LocalMinima;

namespace Navigation2D.NavMath.VattiMerge
{
    public static class VattiMerge
    {
        private static SortedList<float, float> _sbl = new();
        private static List<Tuple<List<LineSegment2D>, List<LineSegment2D>>> _lml = new();
        private static  SortedList<VattiEdge, float> _ael = new();
        private static List<Shape2D> _output = new();
        public static List<Shape2D> GetMergedShape(Shape2D a, Shape2D b)
        {
            //TODO merge all connected collinear edges
            
            _sbl = new();
            _lml = new();
            _ael = new();
            _output = new();
            float yb = 0, yt = 0;

            _updateLMLAndSBL(a, false);
            _updateLMLAndSBL(b, true);
            yb = _sbl[0];
            _sbl.RemoveAt(0);

            while (_sbl.Count > 0)
            {
                
            }
            
            throw new NotImplementedException();
        }

        private static void _updateLMLAndSBL(Shape2D shape, bool IsClip)
        {
            var lml = LocalMinimaList.GetLocalMinimaList(shape);
            foreach (var pair in lml)
            {
                var startY = pair.Item1[0].P1.y;
                //adding first nonhorizontal edges to sbl
                var topY1 = pair.Item1.First(x=>x.P1.y != x.P2.y).P2.y;
                var topY2 = pair.Item2.First(x=>x.P1.y != x.P2.y).P2.y;
                if(!_sbl.ContainsKey(startY))
                    _sbl.Add(startY, startY);
                if(!_sbl.ContainsKey(topY1))
                    _sbl.Add(topY1, topY1);
                if(!_sbl.ContainsKey(topY2))
                    _sbl.Add(topY2, topY2);
            }
        }

        private static void _addNewBoundPairs( float y)
        {
            while (!(_lml.Count > 0 && _lml[0].Item1[0].P1.y == y))
            {
                _addEdgesToAEL(_lml[0], y);
                _lml.RemoveAt(0);
            }
        }

        private static void _addEdgesToAEL(Tuple<List<LineSegment2D>, List<LineSegment2D>> pair, float yb)
        {
            var topY1 = pair.Item1.First(x=>x.P1.y != x.P2.y).P2.y;
            var topY2 = pair.Item2.First(x=>x.P1.y != x.P2.y).P2.y;
        }
    }
}