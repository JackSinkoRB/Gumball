using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using PlayFab;
using PlayFab.PfEditor.EditorModels;
using UnityEngine;
using GetTitleDataRequest = PlayFab.ClientModels.GetTitleDataRequest;

namespace Gumball
{
    public sealed class PlayFabTitleDataProvider : DataProvider
    {

        public PlayFabTitleDataProvider(string identifier) : base(identifier)
        {
            
        }

        public override bool SourceExists()
        {
            return true; //PlayFab source always exists, however may have connection error while loading the source
        }
        
        protected override void SaveToSource()
        {
            //cannot save to PlayFab title data directly
            //it should be accessed using the PlayFab website 
        }

        protected override void LoadFromSource()
        {
        }

        protected override void OnRemoveFromSource()
        {
            //cannot remove PlayFab title data directly
            //it should be accessed using the PlayFab website 
        }

    }
}