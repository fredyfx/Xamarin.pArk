using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.AVFoundation;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreLocation;
using MonoTouch.CoreMotion;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Xamarin.pArk
{
    public class ARView : UIView
    {
        UIView captureView;

        AVCaptureSession captureSession;
        AVCaptureVideoPreviewLayer captureLayer;

        CMMotionManager motionManager;


        float[] projectionTransform;
        float[] cameraTransform;
        List<float[]> placesOfInterestCoordinates;

        public List<PlaceOfInterest> PlacesOfInterest
        {
            get;
            set;
        }

        public ARView()
        {
            captureView = new UIView(UIScreen.MainScreen.Bounds);
            AddSubview(captureView);
            SendSubviewToBack(captureView);

            projectionTransform = new float[16];
            MathHelpers.CreateProjectionMatrix(ref projectionTransform, (float)(UIScreen.MainScreen.Bounds.Size.Width * 1.0 / UIScreen.MainScreen.Bounds.Size.Height), 0.25f, 1000.0f);
        }

        public void Start()
        {
            StartCameraPreview();
            StartDeviceMotion();
        }
            
        public void Stop()
        {
            StopCameraPreview();
            StopDeviceMotion();
        }

       
        private void StartCameraPreview()
        {
            captureSession = new AVCaptureSession();
            captureSession.AddInput(AVCaptureDeviceInput.FromDevice(MediaDevices.BackCamera));

            captureLayer = new AVCaptureVideoPreviewLayer(captureSession);
            captureLayer.Frame = captureView.Bounds;

            captureView.Layer.AddSublayer(captureLayer);

            captureSession.StartRunning();
        }
            
        public void UpdatePlacesOfInterestCoordinates(CLLocation newLocation)
        {
            double myX = 0.0, myY = 0.0, myZ = 0.0;
            MathHelpers.LatLonToEcef(newLocation.Coordinate.Latitude, newLocation.Coordinate.Longitude, 0.0, ref myX, ref myY, ref myZ);

            placesOfInterestCoordinates = new List<float[]>();

            foreach (var poi in PlacesOfInterest)
            {
                double poiX = 0.0, poiY = 0.0, poiZ = 0.0, e = 0.0, n = 0.0, u = 0.0;
                MathHelpers.LatLonToEcef(poi.Location.Coordinate.Latitude, poi.Location.Coordinate.Longitude, 0.0, ref poiX, ref poiY, ref poiZ);
                MathHelpers.EcefToEnu(poi.Location.Coordinate.Latitude, poi.Location.Coordinate.Longitude, myX, myY, myZ, poiX, poiY, poiZ, ref e, ref n, ref u);

                var p = new float[4];
                p[0] = (float)n;
                p[1] = -(float)e;
                p[2] = 0.0f;
                p[3] = 1.0f;

                placesOfInterestCoordinates.Add(p);

                if (poi.View == null)
                {
                    var label = new UILabel
                    {
                        AdjustsFontSizeToFitWidth = false,
                        Opaque = false,
                        BackgroundColor = new UIColor(0.1f, 0.1f, 0.1f, 0.5f),
                        Center = new PointF(200.0f, 200.0f),
                        TextAlignment = UITextAlignment.Center,
                        TextColor = UIColor.White,
                        Lines = 0,
                        LineBreakMode = UILineBreakMode.WordWrap,
                        Hidden = true
                    };

                    poi.View = label;
                    AddSubview(poi.View);
                }
                var distance = newLocation.DistanceFrom(new CLLocation(poi.Location.Coordinate.Latitude, poi.Location.Coordinate.Longitude));
                if (distance > 1000)
                {
                    ((UILabel)poi.View).Text = string.Format("{0} - {1:F} km", poi.Name, distance/1000);
                }
                else
                {
                    ((UILabel)poi.View).Text = string.Format("{0} - {1:F} m", poi.Name, distance);
                }
                var size = ((UILabel)poi.View).StringSize(((UILabel)poi.View).Text, ((UILabel)poi.View).Font);
                ((UILabel)poi.View).Bounds = new RectangleF(0.0f, 0.0f, size.Width, size.Height);
            }
        }

        public override void Draw(RectangleF rect)
        {
            if (placesOfInterestCoordinates == null)
            {
                return;
            }

            var projectionCameraTransform = new float[16];
            MathHelpers.MultiplyMatrixAndMatrix(ref projectionCameraTransform, projectionTransform, cameraTransform);

            for (int i = 0; i < PlacesOfInterest.Count; i++)
            {
                var poi = PlacesOfInterest[i];

                var v = new float[4];
                MathHelpers.MultiplyMatrixAndVector(ref v, projectionCameraTransform, placesOfInterestCoordinates[i]);

                float x = (v[0] / v[3] + 1.0f) * 0.5f;
                float y = (v[1] / v[3] + 1.0f) * 0.5f;

                if (v[2] < 0.0f)
                {
                    poi.View.Center = new PointF(x * Bounds.Size.Width, Bounds.Size.Height - y * Bounds.Size.Height);
                    poi.View.Hidden = false;
                }
                else
                {
                    poi.View.Hidden = true;
                }
            }
        }

        private void StartDeviceMotion()
        {
            motionManager = new CMMotionManager
            {
                ShowsDeviceMovementDisplay = true,
                DeviceMotionUpdateInterval = 1.0/60.0
            };
            //motionManager.StartDeviceMotionUpdates(CMAttitudeReferenceFrame.XTrueNorthZVertical);
            motionManager.StartDeviceMotionUpdates(CMAttitudeReferenceFrame.XTrueNorthZVertical, NSOperationQueue.CurrentQueue, (motion, error) =>
            {
                if(motion != null)
                {
                    cameraTransform = new float[16];
                    MathHelpers.TransformFromCMRotationMatrix(ref cameraTransform, motion.Attitude.RotationMatrix);
                    SetNeedsDisplay();
                }
            });
        }
            
        void UpdateDisplay()
        {
            CMDeviceMotion motion = motionManager.DeviceMotion;
            if (motion != null)
            {
                cameraTransform = new float[16];
                MathHelpers.TransformFromCMRotationMatrix(ref cameraTransform, motion.Attitude.RotationMatrix);

                SetNeedsDisplay();
            }
        }

        private void StopCameraPreview()
        {
            captureSession.StopRunning();
        }
            
        private void StopDeviceMotion()
        {
            motionManager.StopDeviceMotionUpdates();
        }
    }
}