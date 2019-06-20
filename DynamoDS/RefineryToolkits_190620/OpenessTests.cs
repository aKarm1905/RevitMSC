﻿using NUnit.Framework;
using Autodesk.RefineryToolkits.SpacePlanning.Analyze;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestServices;
using Autodesk.DesignScript.Geometry;

namespace Autodesk.RefineryToolkits.SpacePlanning.Analyze.Tests
{
    [TestFixture]
    public class OpenessTests : GeometricTestBase
    {
        private static Polygon obstacle;

        [SetUp]
        public void BeforeTest()
        {
            obstacle = Rectangle.ByWidthLength(10, 10) as Polygon;
        }

        /// <summary>
        /// Check if the openess score detects a obstacle on the left
        /// </summary>
        [Test]
        public void LeftObstacleTest()
        {
            // Create a rectangle object with 4 equal sides to check openess score
            Point origin = Point.ByCoordinates(-10, 0);
            Surface surface = Surface.ByPatch(Rectangle.ByWidthLength(CoordinateSystem.ByOrigin(origin), 10, 10));

            // Calculate openess score
            double openessScore = Openess.FromSurface(surface,0,new List<Polygon> { }, new List<Polygon> { obstacle });

            // Check of score equals 0.25, as the entire left side is blocked by the obstacle
            Assert.AreEqual(0.25, openessScore);

            // Dispose unused geometry
            obstacle.Dispose();
            origin.Dispose();
        }

        /// <summary>
        /// Check if the openess score detects a obstacle on the right
        /// </summary>
        [Test]
        public void RightObstacleTest()
        {
            // Create a rectangle object with 4 equal sides to check openess score
            Point origin = Point.ByCoordinates(10, 0);
            Surface surface = Surface.ByPatch(Rectangle.ByWidthLength(CoordinateSystem.ByOrigin(origin), 10, 10));

            // Calculate openess score
            double openessScore = Openess.FromSurface(surface, 0, new List<Polygon> { }, new List<Polygon> { obstacle });

            // Check of score equals 0.25, as the entire right side is blocked by the obstacle
            Assert.AreEqual(0.25, openessScore);

            // Dispose unused geometry
            obstacle.Dispose();
            origin.Dispose();
        }

        /// <summary>
        /// Check if the openess score detects a obstacle on the top
        /// </summary>
        [Test]
        public void TopObstacleTest()
        {
            // Create a rectangle object with 4 equal sides to check openess score
            Point origin = Point.ByCoordinates(0, 10);
            Surface surface = Surface.ByPatch(Rectangle.ByWidthLength(CoordinateSystem.ByOrigin(origin), 10, 10));

            // Calculate openess score
            double openessScore = Openess.FromSurface(surface, 0, new List<Polygon> { }, new List<Polygon> { obstacle });

            // Check of score equals 0.25, as the entire top is blocked by the obstacle
            Assert.AreEqual(0.25, openessScore);

            // Dispose unused geometry
            obstacle.Dispose();
            origin.Dispose();
        }

        /// <summary>
        /// Check if the openess score detects a obstacle on the bottom
        /// </summary>
        [Test]
        public void BottomObstacleTest()
        {
            // Create a rectangle object with 4 equal sides to check openess score
            Point origin = Point.ByCoordinates(0, -10);
            Surface surface = Surface.ByPatch(Rectangle.ByWidthLength(CoordinateSystem.ByOrigin(origin), 10, 10));

            // Calculate openess score
            double openessScore = Openess.FromSurface(surface, 0, new List<Polygon> { }, new List<Polygon> { obstacle });

            // Check of score equals 0.25, as the entire bottom is blocked by the obstacle
            Assert.AreEqual(0.25, openessScore);
            
            // Dispose unused geometry
            obstacle.Dispose();
            origin.Dispose();
        }

        /// <summary>
        /// Check if the openess score detects a obstacle on the left and right
        /// </summary>
        [Test]
        public void ObstacleOnBothSides()
        {
            List<Polygon> obstaclePolygons = new List<Polygon>()
            {
                obstacle.Translate(-10) as Polygon,
                obstacle.Translate(10) as Polygon
            };

            // Create a rectangle object with 4 equal sides to check openess score
            Surface surface = Surface.ByPatch(Rectangle.ByWidthLength(10, 10));

            // Calculate openess score
            double openessScore = Openess.FromSurface(surface, 0, new List<Polygon> { }, obstaclePolygons);

            // Check if score equals 0.50, as the entire right and left side is blocked by the obstacles
            Assert.AreEqual(0.50, openessScore);

            // Dispose unused geometry
            obstaclePolygons.ForEach(poly => poly.Dispose());
        }
    }
}