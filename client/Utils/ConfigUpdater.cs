using System;
using Newtonsoft.Json;
using SPTLeaderboard.Data;
using SPTLeaderboard.Models;

namespace SPTLeaderboard.Utils
{
    public static class ConfigUpdater
    {
        public static void UpdateEquipmentLimits()
        {
            try
            {
                EquipmentData newConfig;
                var request = NetworkApiRequestModel.CreateGet(GlobalData.ConfigUrl);
                request.OnSuccess = (response, code) =>
                {
                    newConfig = JsonConvert.DeserializeObject<EquipmentData>(response);
                
                    if (newConfig != null)
                    {
                        LeaderboardPlugin.logger.LogInfo($"Request GET OnSuccess {response}");
                    
#if DEBUG
                        LeaderboardPlugin.logger.LogInfo($"{JsonConvert.SerializeObject(newConfig)}");
#endif
                    
                        if(newConfig.TacticalVest <= 0 || newConfig.Pockets <= 0 || newConfig.Backpack <= 0 || newConfig.SecuredContainer <= 0 || newConfig.Stash <= 0 )
                            return;
                    
                        GlobalData.EquipmentLimits = new EquipmentData
                        {
                            TacticalVest = newConfig.TacticalVest,
                            Pockets = newConfig.Pockets,
                            Backpack = newConfig.Backpack,
                            SecuredContainer = newConfig.SecuredContainer,
                            Stash = newConfig.Stash
                        };
                    
                        LeaderboardPlugin.Instance.configUpdated = true;
                    }
                };
                request.Send();
            }
            catch (Exception ex)
            {
                LeaderboardPlugin.logger.LogWarning($"Error update config: {ex.Message}");
            }
        }
    }
}