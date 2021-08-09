
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor.U2D.Path.GUIFramework;
    using UnityEngine;

    public class OpenUniformBSpline : Curve
    {
        private List<ControlVertex> controlVertices = new List<ControlVertex>();

        private ControlVertex lastVertex;
        private float lengthEstimate = 0;

        private const float meshSize = 0.4f;

        private int curveOrder;
        private int targetCurveOrder = 7;

        private List<Vector3[]> samplePointPositions = new List<Vector3[]>();

        void AddVertex(Vector2 position)
        {
            // Create a new vertex object
            GameObject vertexObject = Instantiate(controlVertexPrefab, position, Quaternion.identity, transform);
            ControlVertex vertex = vertexObject.GetComponent<ControlVertex>();

            // Estimate the curve length
            if (controlVertices.Count > 0)
            {
                Vector2 lastPosition = lastVertex.transform.position;
                float addedLengthEstimate = Mathf.Sqrt(Mathf.Pow(position.x - lastPosition.x, 2) +
                                                       Mathf.Pow(position.y - lastPosition.y, 2));
                lengthEstimate += addedLengthEstimate;
            }
            
            // Add vertex to the vertex list
            controlVertices.Add(vertex);
            samplePointPositions.Add(new Vector3[0]);

            lastVertex = vertex;
        }
        
        public override void AddNodeAtPosition(Vector2 position)
        {
            AddVertex(position);
            
            // Don't attempt to draw the curve if only one vertex exists
            if (controlVertices.Count == 1)
            {
                return;
            }
            
            // Increase the curve order if it is below target, and resample as much of the curve as is necessary
            if (curveOrder < targetCurveOrder)
            {
                curveOrder = Mathf.Min(targetCurveOrder, controlVertices.Count);
                UpdateAllSamplePoints();
            }
            else
            {
                UpdateSamplePointsAroundControlPoint(controlVertices.Count - 1);
            }
            DrawCurve();
        }

        public override void UpdateNodeAtIndex(int index, Vector2 position)
        {
            controlVertices[index].transform.position = position;
            if (controlVertices.Count == 1)
            {
                return;
            }
            UpdateSamplePointsAroundControlPoint(index);
            DrawCurve();
        }

        public override void UpdateLastNode(Vector2 position)
        {
            UpdateNodeAtIndex(controlVertices.Count - 1, position);
        }

        private float ComputeWeight(float param, int cvIndex)
        {
            // Generate appropriate knot array for the control vertex
            int[] knots = new int[curveOrder + 1];

            for (int i = 0; i <= curveOrder; ++i)
            {
                knots[i] = cvIndex - curveOrder + 1 + i;
                if (knots[i] < 0)
                {
                    knots[i] = 0;
                } 
                else if (knots[i] > controlVertices.Count - curveOrder + 1)
                {
                    knots[i] = controlVertices.Count - curveOrder + 1;
                }
            }
            
            return ComputeWeightFromKnots(param, knots);
        }

        private float ComputeWeightFromKnots(float param, int[] knots)
        {
            // Return step function if only two knots are given
            if (knots.Length == 2)
            {
                if (knots[0] <= param && param < knots[1])
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                // Take weighted average of the weights given by removing the first or last knot
                float weight1, weight2;
                if (knots[knots.Length - 2] - knots[0] <= float.Epsilon)
                {
                    weight1 = 0;
                }
                else
                {
                    weight1 = (param - knots[0]) / (knots[knots.Length - 2] - knots[0]);
                }
                
                if (knots[knots.Length - 1] - knots[1] <= float.Epsilon)
                {
                    weight2 = 0;
                }
                else
                {
                    weight2 = (knots[knots.Length - 1] - param) / (knots[knots.Length - 1] - knots[1]);
                }

                int[] earlyKnots = knots.Take(knots.Length - 1).ToArray();
                int[] lateKnots = knots.Skip(1).Take(knots.Length - 1).ToArray();

                return weight1 * ComputeWeightFromKnots(param, earlyKnots) +
                       weight2 * ComputeWeightFromKnots(param, lateKnots);
            }
        }

        private void DrawCurve()
        {
            // Compute total number of sample points, including a final point at the end of the curve
            int totalSamplePointNumber = 0;
            for (int segmentIndex = 0; segmentIndex < samplePointPositions.Count; ++segmentIndex)
            {
                totalSamplePointNumber += samplePointPositions[segmentIndex].Length;
            }

            ++totalSamplePointNumber;

            // Create array of all sample points
            Vector3[] positions = new Vector3[totalSamplePointNumber];

            int overallPointIndex = 0;
            foreach (Vector3[] segment in samplePointPositions)
            {
                foreach (Vector3 position in segment)
                {
                    positions[overallPointIndex] = position;
                    ++overallPointIndex;
                }
            }

            positions[totalSamplePointNumber - 1] = controlVertices[controlVertices.Count - 1].transform.position - transform.position;

            // Set line renderer points
            lineRenderer.positionCount = totalSamplePointNumber;
            lineRenderer.SetPositions(positions);
        }

        private void UpdateAllSamplePoints()
        {
            for (int segmentIndex = 0; segmentIndex <= controlVertices.Count - curveOrder; ++segmentIndex)
            {
                UpdateSamplePointsInSegment(segmentIndex);
            }
        }

        private void UpdateSamplePointsAroundControlPoint(int cvIndex)
        {
            // Determine which segments to update
            int lastIndex = Mathf.Max(cvIndex, curveOrder);
            lastIndex = Mathf.Min(lastIndex, controlVertices.Count - curveOrder);

            int firstIndex = Mathf.Max(lastIndex - curveOrder, 0);

            for (int segmentIndex = firstIndex; segmentIndex <= lastIndex; ++segmentIndex)
            {
                UpdateSamplePointsInSegment(segmentIndex);
            }
        }

        private void UpdateSamplePointsInSegment(int segmentIndex)
        {
            // Calculate how many points to sample and initialise the position array for the segment
            int numSamplePoints = Mathf.CeilToInt(EstimateSegmentLength(segmentIndex) / meshSize);
            samplePointPositions[segmentIndex] = new Vector3[numSamplePoints];

            float sampleSpacing = 1f / numSamplePoints;
            float sampleParameter = segmentIndex;

            // Compute sample points as a weighted sum of the appropriate control vertices
            for (int i = 0; i < numSamplePoints; ++i)
            {
                Vector2 samplePoint = Vector2.zero;
            
                for (int controlVertexIndex = segmentIndex; controlVertexIndex < segmentIndex + curveOrder; ++controlVertexIndex)
                {
                    samplePoint += ComputeWeight(sampleParameter, controlVertexIndex) * (Vector2)controlVertices[controlVertexIndex].transform.position;
                }

                samplePointPositions[segmentIndex][i] = (Vector3)samplePoint - transform.position;

                sampleParameter += sampleSpacing;
            }
        }
        
        private float EstimateSegmentLength(int segmentIndex)
        {
            float lengthEstimate = 0;
            
            int numSamplePoints = 10;

            float sampleSpacing = 1f / numSamplePoints;
            float sampleParameter = segmentIndex;

            Vector2 prevSamplePoint = Vector2.zero;

            // Sum distances between a few sparsely placed sample points to estimate curve length
            for (int i = 0; i < numSamplePoints; ++i)
            {
                Vector2 samplePoint = Vector2.zero;
            
                for (int controlVertexIndex = segmentIndex; controlVertexIndex < segmentIndex + curveOrder; ++controlVertexIndex)
                {
                    samplePoint += ComputeWeight(sampleParameter, controlVertexIndex) * (Vector2)controlVertices[controlVertexIndex].transform.position;
                }

                if (i >= 1)
                {
                    lengthEstimate += (samplePoint - prevSamplePoint).magnitude;
                }

                prevSamplePoint = samplePoint;

                sampleParameter += sampleSpacing;
            }

            // Estimate distance to the end of the curve as the average of the sampled distances
            return numSamplePoints * lengthEstimate / (numSamplePoints - 1);
        }
        
    }