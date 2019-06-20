﻿using Autodesk.DesignScript.Geometry;
using Autodesk.RefineryToolkits.Core.Geometry.Extensions;
using Autodesk.RefineryToolkits.Core.Utillites;
using Dynamo.Graph.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate
{
    public static class DeskLayout
    {
        /// <summary>
        /// Creates a layout of desks on a surface based on desk dimensions
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="deskWidth">the width of the desks in mm</param>
        /// <param name="deskDepth">the depth of the desks in mm</param>
        /// <param name="backToBack">the distance in mm between two desks where the people sat at them are back to back</param>
        /// <search></search>
        [NodeCategory("Create")]
        public static object Create(
            Surface surface,
            double deskWidth = 1400,
            double deskDepth = 800,
            double backToBack = 2200)
        {
            Surface boundingSrf = surface.BoundingSurface();
            List<Curve> perimCrvs = boundingSrf.PerimeterCurves().ToList();

            Curve max;
            List<Curve> others;
            Dictionary<string, dynamic> dict = perimCrvs.MaximumLength();
            if (dict["maxCrv"].Count < 1)
            {
                max = dict["otherCrvs"][0] as Curve;
                int count = dict["otherCrvs"].Count;
                List<Curve> rest = dict["otherCrvs"];
                others = rest.GetRange(1, (count - 1));
            }
            else
            {
                max = dict["maxCrv"][0] as Curve;
                others = dict["otherCrvs"];
            }

            Point comPt = max.StartPoint;

            List<List<bool>> boolList = new List<List<bool>>();
            foreach (Curve crv in others)
            {
                List<bool> subList = new List<bool>();
                subList.Add(comPt.CompareCoincidental(crv.StartPoint));
                subList.Add(comPt.CompareCoincidental(crv.EndPoint));
                boolList.Add(subList);
            }

            List<bool> mask1 = new List<bool>();
            foreach (List<bool> subList in boolList)
            {
                mask1.Add(!subList.Any(c => c == true));
            }

            var transBoolList = boolList.SelectMany(inner => inner.Select((item, index) => new { item, index })).GroupBy(i => i.index, i => i.item).Select(g => g.ToList()).ToList();
            List<bool> mask2 = new List<bool>();
            foreach (List<bool> subList in transBoolList)
            {
                mask2.Add(!subList.Any(c => c == true));
            }

            List<string> strList = new List<string> { "start", "end" };
            Curve crv1 = others.Zip(mask2, (name, filter) => new { name, filter, }).Where(item => item.filter == true).Select(item => item.name).ToList()[0];
            string dir = strList.Zip(mask1, (name, filter) => new { name, filter, }).Where(item => item.filter == true).Select(item => item.name).ToList()[0];

            if (dir == "end")
            {
                crv1 = crv1.Reverse();
            }

            double crvLen = crv1.Length;
            Vector vec = Vector.ByTwoPoints(crv1.StartPoint, crv1.EndPoint);
            bool vecCom = vec.X == 0;

            List<double> dims;
            if (vecCom)
            {
                dims = new List<double> { deskWidth, deskDepth };
            }
            else
            {
                dims = new List<double> { deskDepth, deskWidth };
            }

            double halfb2b = backToBack / 2;
            double halfDeskWidth = deskWidth / 2;
            double halfDeskDepth = deskDepth / 2;

            List<double> lstA = new List<double>
            {
                deskDepth,
                halfDeskDepth+halfDeskDepth+backToBack
            };

            int amount = Convert.ToInt32(System.Math.Round(crvLen / lstA.Sum() + 1));
            var repeatLst = Enumerable.Repeat(lstA, amount).ToList();
            var flatLst = repeatLst.SelectMany(i => i).ToList();
            flatLst.Insert(0, halfb2b + halfDeskDepth);

            List<double> partials = flatLst.RunningTotals();

            List<bool> mask3 = new List<bool>();
            foreach (double p in partials)
            {
                mask3.Add(p > crvLen);
            }

            List<double> partialFalse = partials.Zip(mask3, (name, filter) => new { name, filter, }).Where(item => item.filter == false).Select(item => item.name).ToList();
            List<Curve> transGeo = new List<Curve>();
            foreach (double dis in partialFalse)
            {
                transGeo.Add(max.Translate(vec, dis) as Curve);
            }

            double crvLen2 = transGeo[0].Length;

            List<double> offsetNum = new List<double> { deskWidth };
            var offsetNums = Enumerable.Repeat(offsetNum, 100).ToList();
            var offsetNumFlat = offsetNums.SelectMany(i => i).ToList();
            offsetNumFlat.Insert(0, halfDeskWidth);

            List<double> partialsOffsets = offsetNumFlat.RunningTotals();

            List<bool> mask4 = new List<bool>();
            foreach (double p in partialsOffsets)
            {
                if (p > crvLen2)
                {
                    mask4.Add(true);
                }
                else
                {
                    mask4.Add(false);
                }
            }

            List<double> offsetFalse = partialsOffsets.Zip(mask4, (name, filter) => new { name, filter, }).Where(item => item.filter == false).Select(item => item.name).ToList();
            double lastItem = offsetFalse[offsetFalse.Count - 1] + halfDeskWidth;

            List<double> remLastItem = offsetFalse.ToList();
            remLastItem.RemoveAt(remLastItem.Count - 1);

            List<double> segLengths;
            if (lastItem > crvLen2)
            {
                segLengths = remLastItem;
            }
            else
            {
                segLengths = offsetFalse;
            }

            List<Rectangle> rectList = new List<Rectangle>();
            foreach (Curve c in transGeo)
            {

                foreach (double seg in segLengths)
                {
                    Point pt = c.PointAtSegmentLength(seg);
                    CoordinateSystem cs = CoordinateSystem.ByOrigin(pt);
                    rectList.Add(Rectangle.ByWidthLength(cs, dims[0], dims[1]));
                    pt.Dispose();
                    cs.Dispose();
                }
            }

            //Create outer surface for intersection
            Point centerPt = boundingSrf.PointAtParameter(0.5, 0.5);
            Plane pln = Plane.ByOriginNormal(centerPt, Vector.ZAxis());
            Surface scSrf = boundingSrf.Scale(pln, 2, 2, 1) as Surface;
            Surface scSplit = scSrf.Split(surface)[0] as Surface;

            List<Rectangle> cleanRect = new List<Rectangle>();
            foreach (Rectangle r in rectList)
            {
                if (r.DoesIntersect(scSplit))
                {
                    r.Dispose();
                }
                else
                {
                    cleanRect.Add(r);
                }
            }

            //Dispose redundant geometry
            boundingSrf.Dispose();
            max.Dispose();
            comPt.Dispose();
            crv1.Dispose();
            vec.Dispose();
            centerPt.Dispose();
            pln.Dispose();
            scSrf.Dispose();
            scSplit.Dispose();


            return cleanRect;

        }
    }
}
