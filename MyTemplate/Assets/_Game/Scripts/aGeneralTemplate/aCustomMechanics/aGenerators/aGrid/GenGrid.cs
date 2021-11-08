using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace Generators
{
    public class GenGrid : MonoBehaviour
    {
        [Header("GridParameters")]
        [Space]
        [SerializeField]
        private int numberOfRows;
        [SerializeField]
        private int numberOfColumns;

        [Header("TriangularParameters")]
        [Space]
        [SerializeField]
        [Tooltip("Currently not used in calcs. Radius of outer circle")]
        private float triangleSize = 1;
        [SerializeField]
        private float widthBetween = 1;
        [SerializeField]
        private float gapBetweenRowsTri = 0.2f;
        [SerializeField]
        private float heightBetwAdjacentTri = 0.5f;

        [Header("SquareParameters")]
        [Space]
        [SerializeField]
        [Tooltip("The radius of outer circle is the half of this value")]
        private float squareSize = 1;
        [SerializeField]
        private float gapBetweenRows = 0.2f;
        [SerializeField]
        private float gapBetweenColumns = 0.2f;

        [Header("HexagonalParameters")]
        [Space]
        [SerializeField]
        [Tooltip("Radius of outer circle")]
        private float hexagonSize = 1f;
        [SerializeField]
        private float gapSize = 0.2f;

        [Header("Tile")]
        [Space]
        [SerializeField]
        private GameObject tilePrefab;
        [SerializeField]
        private Transform parentForTiles;

        private void Start()
        {
            GenerateHexagonalGrid(transform.position);
        }

        public void GenerateTriangularGrid(Vector3 gridPos)
        {
            List<List<Tile>> tiles = new List<List<Tile>>();

            //Row displacement based on orientation
            float scaleToUp = heightBetwAdjacentTri * 2; 
            float scaleToDown = heightBetwAdjacentTri * 4;

            float zInitPos = -1 * 
                (gapBetweenRowsTri * (numberOfRows - 1) / 2.0f +
                numberOfRows / 2 * scaleToDown +
                (numberOfRows % 2 * scaleToDown) +
                numberOfRows / 2 * scaleToUp) / 2;

            Vector3 initTilePos = new Vector3(
                -widthBetween * (numberOfColumns - 1) / 2.0f,
                0,
                zInitPos);

            Vector3 rowStartTilePos = initTilePos;
            Vector3 tilePos = initTilePos;

            Vector3 horizDisplacement = widthBetween * Vector3.right;
            Vector3 upDisplacement = heightBetwAdjacentTri * Vector3.forward;
            Vector3 downDisplacement = -upDisplacement;

            Vector3 rowDisplacementToUpOrient = (scaleToUp + gapBetweenRowsTri) * Vector3.forward;
            Vector3 rowDisplacementToDownOrient = (scaleToDown + gapBetweenRowsTri) * Vector3.forward;

            Quaternion rotationUp = Quaternion.Euler(new Vector3(0, 0, 0));
            Quaternion rotationDown = Quaternion.Euler(new Vector3(0, 180, 0));

            Quaternion tileRot = rotationUp;
            OrientType tileOrient = OrientType.Up;
            OrientType rowStartTileOrient = OrientType.Up;

            for (int row = 0; row < numberOfRows; row++)
            {
                tiles.Add(new List<Tile>());
                for (int column = 0; column < numberOfColumns; column++)
                {
                    tileRot = tileOrient == OrientType.Up ? rotationUp : rotationDown;
                    GameObject tileObj =
                        Instantiate(tilePrefab, tilePos + gridPos, tileRot, parentForTiles);

                    AlternateOrient(ref tileOrient);

                    Tile tile = tileObj.GetComponent<Tile>();
                    if (tile != null)
                    {
                        tiles[row].Add(tile);
                        tile.AssignOrient(tileOrient);
                        tile.AssignIndex(row * numberOfRows + column);
                    }

                    tilePos += horizDisplacement;
                    tilePos += tileOrient == OrientType.Up ?
                        upDisplacement : downDisplacement;
                }

                tilePos = rowStartTilePos;
                tilePos += rowStartTileOrient == OrientType.Up ?
                    rowDisplacementToDownOrient : rowDisplacementToUpOrient;
                rowStartTilePos = tilePos;

                tileOrient = rowStartTileOrient;
                AlternateOrient(ref tileOrient);
                rowStartTileOrient = tileOrient;
            }
        }

        private void AlternateOrient(ref OrientType orient)
        {
            if (orient == OrientType.Up)
            {
                orient = OrientType.Down;
            }
            else if (orient == OrientType.Down)
            {
                orient = OrientType.Up;
            }
        }

        public void GenerateSquareGrid(Vector3 gridPos)
        {
            List<List<Tile>> tiles = new List<List<Tile>>();

            float scalarDeltaX = squareSize + gapBetweenColumns;
            float scalarDeltaZ = squareSize + gapBetweenRows;
            Vector3 horizDisplacement = scalarDeltaX * Vector3.right;
            Vector3 verticalDisplacement = scalarDeltaZ * Vector3.forward;

            Vector3 initTilePos =
                -(numberOfColumns / 2) * horizDisplacement +
                -(numberOfRows / 2) * verticalDisplacement;

            Vector3 rowStartTilePos = initTilePos;
            Vector3 tilePos = initTilePos;

            Quaternion tileRot = Quaternion.identity;

            for (int row = 0; row < numberOfRows; row++)
            {
                tiles.Add(new List<Tile>());
                for (int column = 0; column < numberOfColumns; column++)
                {
                    GameObject tileObj =
                        Instantiate(tilePrefab, tilePos + gridPos, tileRot, parentForTiles);

                    Tile tile = tileObj.GetComponent<Tile>();
                    if (tile != null)
                    {
                        tiles[row].Add(tile);
                        tile.AssignIndex(row * numberOfRows + column);
                    }
                    tilePos += horizDisplacement;
                }
                tilePos = rowStartTilePos;
                tilePos += verticalDisplacement;
                rowStartTilePos = tilePos;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gridPos"></param>
        /// <link="https://www.redblobgames.com/grids/hexagons/#coordinates></link>
        public void GenerateHexagonalGrid(Vector3 gridPos)
        {
            List<List<Tile>> tiles = new List<List<Tile>>();

            // radius of inner circleOfHexagon can help understand

            float cos30 = Mathf.Cos(Mathf.PI / 6);

            float vertGapSize = cos30 * gapSize;

            float innerRadius = cos30 * hexagonSize;
            float columnDistance = innerRadius + gapSize + innerRadius;
            float horizHexDisp = columnDistance / 2; // projection on X axis
            Vector3 horizDisplacement = horizHexDisp * Vector3.right;


            float vertHexDisp = cos30 * innerRadius; // projection on Z axis
            float heightBetweenAdjacentHexagons = vertHexDisp + vertGapSize + vertHexDisp;

            Vector3 upDisplacement = heightBetweenAdjacentHexagons * Vector3.forward;
            Vector3 downDisplacement = -upDisplacement;

            Vector3 rowDisplacement = upDisplacement * 2;

            Vector3 initTilePos =
                 -(numberOfColumns / 2) * horizDisplacement +
                -(numberOfRows / 2) * rowDisplacement;

            Vector3 rowStartTilePos = initTilePos;
            Vector3 tilePos = initTilePos;

            Quaternion tileRot = Quaternion.identity;

            // produces interesting result with hexagon mesh
            //Quaternion tileRot = Quaternion.Euler(0, 90, 0); 

            bool shouldUp = true;

            for (int row = 0; row < numberOfRows; row++)
            {
                tiles.Add(new List<Tile>());
                for (int column = 0; column < numberOfColumns; column++)
                {
                    GameObject tileObj =
                        Instantiate(tilePrefab, tilePos + gridPos, tileRot, parentForTiles);

                    Tile tile = tileObj.GetComponent<Tile>();
                    if (tile != null)
                    {
                        tiles[row].Add(tile);
                        tile.AssignIndex(row * numberOfRows + column);
                    }

                    tilePos += horizDisplacement;
                    tilePos += shouldUp ? upDisplacement : downDisplacement;
                    shouldUp = !shouldUp;
                }
                tilePos = rowStartTilePos;
                tilePos += rowDisplacement;
                rowStartTilePos = tilePos;

                shouldUp = true;
            }
        }
    }
}
