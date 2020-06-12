﻿using RockyHockey.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RockyHockey.MotionCaptureFramework
{
    public class ImagePositionCollector : PositionCollector
    {
        public ImagePositionCollector(ImageProvider imageProvider = null)
        {
            this.imageProvider = imageProvider ?? new CameraReader();
        }

        public override List<TimedCoordinate> GetPuckPositions()
        {
            List<TimedCoordinate> coordinates;

            long timestamp;
            
            while (true)
            {
                timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                List<Task<TimedCoordinate>> detectionTasks = new List<Task<TimedCoordinate>>();
                for (int a = 0; a < 10; a++)
                    detectionTasks.Add(PositionCalculator.ProcessImage(imageProvider.getTimedImage(), imageProvider.SliceImage));

                coordinates = new List<TimedCoordinate>();
                foreach (Task<TimedCoordinate> detectionTask in detectionTasks)
                {
                    detectionTask.Wait();
                    TimedCoordinate detectedPosition = detectionTask.Result;

                    if (detectedPosition != null)
                        coordinates.Add(detectedPosition);
                }

                if (coordinates.Count >= 5)
                    break;
                else
                    Thread.Sleep(Math.Abs((int)(timestamp + 250 - DateTimeOffset.Now.ToUnixTimeMilliseconds())));
            }

            return coordinates;
        }

        public override TimedCoordinate GetPuckPosition()
        {
            TimedCoordinate coordinate;

            do
            {
                Task<TimedCoordinate> detectionTask = PositionCalculator.ProcessImage(imageProvider.getTimedImage(), imageProvider.SliceImage);
                detectionTask.Wait();
                coordinate = detectionTask.Result;
            } while (coordinate == null);

            return coordinate;
        }

        public override void StopMotionCapturing()
        {
            imageProvider.finalize();
        }
    }
}