// Copyright (c) 2002-2003, Sony Computer Entertainment America
// Copyright (c) 2002-2003, Craig Reynolds <craig_reynolds@playstation.sony.com>
// Copyright (C) 2007 Bjoern Graf <bjoern.graf@gmx.net>
// All rights reserved.
//
// This software is licensed as described in the file license.txt, which
// you should have received as part of this distribution. The terms
// are also available at http://www.codeplex.com/SharpSteer/Project/License.aspx.

using System;
using System.Collections.Generic;
using SharpSteer2;

namespace Demo.PlugIns.MultiplePursuit
{
	public class MpPlugIn : PlugIn
	{
		public MpPlugIn(IAnnotationService annotations)
            :base(annotations)
		{
			_allMp = new List<MpBase>();
		}

		public override String Name { get { return "Multiple Pursuit"; } }

		public override float SelectionOrderSortKey { get { return 0.04f; } }

		public override void Open()
		{
			// create the wanderer, saving a pointer to it
            _wanderer = new MpWanderer(Annotations);
			_allMp.Add(_wanderer);

			// create the specified number of pursuers, save pointers to them
			const int PURSUER_COUNT = 30;
			for (int i = 0; i < PURSUER_COUNT; i++)
                _allMp.Add(new MpPursuer(_wanderer, Annotations));
			//pBegin = allMP.begin() + 1;  // iterator pointing to first pursuer
			//pEnd = allMP.end();          // iterator pointing to last pursuer

			// initialize camera
			Demo.SelectedVehicle = _wanderer;
			Demo.Camera.Mode = Camera.CameraMode.StraightDown;
			Demo.Camera.FixedDistanceDistance = Demo.CAMERA_TARGET_DISTANCE;
			Demo.Camera.FixedDistanceVerticalOffset = Demo.CAMERA2_D_ELEVATION;
		}

		public override void Update(float currentTime, float elapsedTime)
		{
			// update the wanderer
			_wanderer.Update(currentTime, elapsedTime);

			// update each pursuer
			for (int i = 1; i < _allMp.Count; i++)
			{
				((MpPursuer)_allMp[i]).Update(currentTime, elapsedTime);
			}
		}

		public override void Redraw(float currentTime, float elapsedTime)
		{
			// selected vehicle (user can mouse click to select another)
			IVehicle selected = Demo.SelectedVehicle;

			// vehicle nearest mouse (to be highlighted)
			IVehicle nearMouse = Demo.VehicleNearestToMouse();

			// update camera
			Demo.UpdateCamera(elapsedTime, selected);

			// draw "ground plane"
			Demo.GridUtility(selected.Position);

			// draw each vehicles
			foreach (MpBase mp in _allMp)
			    mp.Draw();

		    // highlight vehicle nearest mouse
			Demo.HighlightVehicleUtility(nearMouse);
			Demo.CircleHighlightVehicleUtility(selected);
		}

		public override void Close()
		{
			// delete wanderer, all pursuers, and clear list
			_allMp.Clear();
		}

		public override void Reset()
		{
			// reset wanderer and pursuers
			_wanderer.Reset();
			for (int i = 1; i < _allMp.Count; i++) _allMp[i].Reset();

			// immediately jump to default camera position
			Demo.Camera.DoNotSmoothNextMove();
			Demo.Camera.ResetLocalSpace();
		}

		//const AVGroup& allVehicles () {return (const AVGroup&) allMP;}
        public override IEnumerable<IVehicle> Vehicles
		{
			get { return _allMp.ConvertAll<IVehicle>(m => (IVehicle) m); }
		}

		// a group (STL vector) of all vehicles
	    readonly List<MpBase> _allMp;

		MpWanderer _wanderer;
	}
}
