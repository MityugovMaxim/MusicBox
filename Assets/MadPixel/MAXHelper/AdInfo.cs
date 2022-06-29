using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAXHelper {
    public class AdInfo  {
        public string Placement;
        public bool bIsRewarded;
        public bool HasInternet;
        public string Availability;

        public AdInfo(string Placement, bool bIsRewarded, bool HasInternet = true, string Availability = "available") {
            this.HasInternet = HasInternet;
            this.Placement = Placement;
            this.bIsRewarded = bIsRewarded;
            this.Availability = Availability;
        }
    }
}
