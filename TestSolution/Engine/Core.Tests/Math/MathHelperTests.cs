using System.Linq;
using Core.Math;
using NUnit.Framework;
using UnityEngine;

namespace Core.Tests.Math
{
    [TestFixture]
    public class MathHelperTests
    {
        [TestCase("0;0;0","10;0;0",90)]
        [TestCase("0;0;0", "0;0;10", 0)]
        [TestCase("0;0;0", "0;0;-10", 180)]
        [TestCase("0;0;0", "-5;0;0", -90)]
        public void GetAngleBetweenForwardVectorAndDestination(string sourcePosVector,string destPosVector,float expectedAngle)
        {
            Vector3 sourcePos = ParseVector3(sourcePosVector);
            Vector3 destPos = ParseVector3(destPosVector);

            float actualAngle = MathHelper.GetAngleBetweenForwardVectorAndDestination(sourcePos, destPos);
            
            Assert.AreEqual(expectedAngle, actualAngle);
        }

        private static Vector3 ParseVector3(string vectorText)
        {
            float[] values = vectorText.Split(';').Select(float.Parse).ToArray();
            
            return new Vector3(values[0],values[1],values[2]);
        }
    }
}
