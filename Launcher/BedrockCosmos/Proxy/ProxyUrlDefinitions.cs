// =============================================================================
// Bedrock Cosmos - Copyright (c) 2026
//
// This file is part of Bedrock Cosmos, licensed under the MIT License.
// You must read and agree to the terms of the MIT License before using,
// copying, modifying, or distributing this code.
//
// MIT License - Full terms: https://opensource.org/licenses/MIT
// =============================================================================

namespace BedrockCosmos.Proxy
{
    internal static class ProxyUrlDefinitions
    {
        /* Anything listed here has actions taken other than the default
           response replacement when the proxy encounters the endpoint. */
        internal const string PlayfabGetPublishedItemUrl = "https://20ca2.playfabapi.com/Catalog/GetPublishedItem";
        internal const string PlayfabSearchUrl = "https://20ca2.playfabapi.com/Catalog/Search";
        internal const string StoreRootUrl = "https://store.mktpl.minecraft-services.net/api/v2.0/layout/pages/MultiItemPage_StoreRoot";
        internal const string SkinsRootUrl = "https://store.mktpl.minecraft-services.net/api/v2.0/layout/pages/MultiItemPage_SkinsRoot";
        //internal const string PopCultureUrl = "https://store.mktpl.minecraft-services.net/api/v2.0/layout/pages/MultiItemPage_41a3a746-1797-438f-a410-2f5010ee59fa";
        //internal const string TopSellersUrl = "https://store.mktpl.minecraft-services.net/api/v2.0/layout/pages/MultiItemPage_305ac6f4-b84c-4d07-ab93-a966b136ed81";
        //internal const string PopularUrl = "https://store.mktpl.minecraft-services.net/api/v2.0/layout/pages/MultiItemPage_af88faec-3b1a-452e-bf45-fe67e9cdb14d";
        internal const string PauseMenuUrl = "https://store.mktpl.minecraft-services.net/api/v2.0/layout/pages/MultiItemPage_PauseMenu";
        internal const string CubeCraftUrl = "https://store.mktpl.minecraft-services.net/api/v2.0/layout/pages/ServerPage_master_player_account!4B74434622F61126";
        internal const string EnchantedUrl = "https://store.mktpl.minecraft-services.net/api/v2.0/layout/pages/ServerPage_master_player_account!5366842229349662";
        internal const string GalaxiteUrl = "https://store.mktpl.minecraft-services.net/api/v2.0/layout/pages/ServerPage_master_player_account!33171D90895F3788";
        internal const string LifeboatUrl = "https://store.mktpl.minecraft-services.net/api/v2.0/layout/pages/ServerPage_master_player_account!5F3E53D5C111968C";
        internal const string MegaSmpUrl = "https://store.mktpl.minecraft-services.net/api/v2.0/layout/pages/ServerOffers_0ad4e564-8dcd-4b16-bdf7-229f3c9a0cd0";
        internal const string MinevilleUrl = "https://store.mktpl.minecraft-services.net/api/v2.0/layout/pages/ServerOffers_9c3eac4f-c579-4525-80d9-723e705c992d";
        internal const string MobMazeUrl = "https://store.mktpl.minecraft-services.net/api/v2.0/layout/pages/ServerPage_master_player_account!163AC5C76001EEB1";
        internal const string SoulSteelUrl = "https://store.mktpl.minecraft-services.net/api/v2.0/layout/pages/ServerPage_master_player_account!EBFC1BF4FAB55EFB";
        internal const string TheHiveUrl = "https://store.mktpl.minecraft-services.net/api/v2.0/layout/pages/ServerPage_master_player_account!DD292649E51DB5F9";
        internal const string SessionStartUrl = "https://messaging.mktpl.minecraft-services.net/api/v1.0/session/start";
        internal const string MessagesEventUrl = "https://messaging.mktpl.minecraft-services.net/api/v1.0/messages/event";
        internal const string PersonaSkinSelectorUrl = "https://store.mktpl.minecraft-services.net/api/v2.0/layout/pages/MultiItemPage_PersonaSkinSelector";
    }
}