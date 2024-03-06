using Navigation2D.NavMath.PolygonSelfIntersectionCheck;
using NUnit.Framework;
using UnityEditor;

namespace Tests.NavMathTests
{
    public class PolygonSelfIntersectionCheckTests
    {
        [Test]
        public void RunIntersectingShapes()
        {
            var dataset = AssetDatabase.LoadAssetAtPath<PolygonDataset>(PolygonDataset.kPolygonDatasetPath);

            foreach (var item in dataset.items)
            {
                var res = PolygonSelfIntersectionCheck.HasIntersections(item.PointsList);
                Assert.IsTrue(res == item.SelfIntersecting);
            }
        }
    }
}