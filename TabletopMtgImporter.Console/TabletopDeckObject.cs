using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace TabletopMtgImporter
{
    internal class TabletopDeckObject
    {
        public List<ObjectState> ObjectStates;

        public class ObjectState
        {
            public string Name;
            public List<CardReference> ContainedObjects;
            [JsonProperty("DeckIDs")]
            public List<int> DeckIds;
            public Dictionary<int, CardInfo> CustomDeck;
            public Transform Transform = new Transform();
        }

        public class CardReference
        {
            [JsonProperty("CardID")]
            public int CardId;
            public string Name;
            public string Nickname;
            public Transform Transform = new Transform();
        }

        public class CardInfo
        {
            private static readonly Uri DefaultBackUrl = new Uri("https://www.frogtown.me/images/gatherer/CardBack.jpg");

            [JsonProperty("FaceURL")]
            public Uri FaceUrl;
            [JsonProperty("BackURL")]
            public Uri BackUrl = DefaultBackUrl;
            public int NumHeight = 1;
            public int NumWidth = 1;
            public bool BackIsHidden = true;
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class Transform
        {
            public double PosX;
            public double PosY;
            public double PosZ;
            public double RotX;
            public double RotY = 180;
            public double RotZ = 180;
            public double ScaleX = 1;
            public double ScaleY = 1;
            public double ScaleZ = 1;
        }
    }
}
