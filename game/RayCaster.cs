using System;
using Microsoft.Xna.Framework;

namespace game
{
    public class RayCaster
    {
        public struct HitData
        {
            public Vector2 normal;
            public Vector2 tileCoordinates;
            public float cellFraction;
            public float rayLength;
        }

        public static bool RayIntersectsGrid(Vector2 position, float angle, int cellSize,
            out HitData hitData, Func<HitData, bool> isSolid, float maxViewDistance = 1024)
        {
            float cos = (float) Math.Cos(angle);
            float sin = (float) Math.Sin(angle);
            int signX = Math.Sign(cos);
            int signY = Math.Sign(sin);

            Vector2 tileCoords = new Vector2((float) Math.Floor(position.X / cellSize),
                (float) Math.Floor(position.Y / cellSize));
            
            Vector2 cellVector = new Vector2(cellSize);
            Vector2 minBounds = tileCoords * cellVector;
            Vector2 maxBounds = tileCoords * cellVector + cellVector;
            
            // get the initial ray lengths for intersection with x and y axis
            float tVertical = ((signX < 0 ? minBounds.X : maxBounds.X) - position.X) / cos;
            float tHorizontal = ((signY < 0 ? minBounds.Y : maxBounds.Y) - position.Y) / sin;

            // get delta ray lengths(slope values) per increase in cellsize
            // needs to be positive always
            float deltaVertical = Math.Abs(cellSize / cos);
            float deltaHorizontal = Math.Abs(cellSize / sin);

            hitData = new HitData {normal = new Vector2(cos, sin), rayLength = 0.0f, tileCoordinates = tileCoords};
            while (hitData.rayLength < maxViewDistance)
            {
                if (isSolid.Invoke(hitData))
                    return true;

                if (tVertical <= tHorizontal)
                {
                    tileCoords.X += signX;
                    hitData.rayLength = tVertical;                    
                    hitData.normal = -Vector2.UnitX * signX;
                    hitData.tileCoordinates = tileCoords;
                    hitData.cellFraction = (position.Y + tVertical * sin) % cellSize / cellSize;
                    
                    tVertical += deltaVertical;
                }
                else
                {
                    tileCoords.Y += signY;
                    hitData.rayLength = tHorizontal;                   
                    hitData.normal = -Vector2.UnitY * signY;
                    hitData.tileCoordinates = tileCoords;
                    hitData.cellFraction = (position.X + tHorizontal * cos) % cellSize / cellSize;
                    
                    tHorizontal += deltaHorizontal;
                }
            }
            
            return false;
        }
    }
}