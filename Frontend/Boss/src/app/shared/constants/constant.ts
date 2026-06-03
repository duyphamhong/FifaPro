export class Api{
  public static LOGIN = "/api/Authentication/Login";
  public static REGISTER = "/api/Authentication/register";
  public static GET_NEXT_MATCH = "/api/match/next-match";
  public static GET_PLAYER_POSITION = "/api/user/players-position";
  public static PREDICT_MATCH = "/api/match/set-predict";
  public static GET_PREVIOUS_MATCH = "/api/match/previous-match";
  public static GET_MATCHES = "/api/match/matches";
  public static GET_USER_INFO = "/api/user/player-info";
  public static ADD_USER_ADDITIONAL_INFORMATION = "/api/user/additional-information";
  public static UPDATE_AVATAR = "/api/user/avatar";
  public static CHANGE_PASS = "/api/Authentication/change-password";
  public static SET_CHAMPION = "/api/user/set-champion";
  public static GET_HISTORY = "/api/user/prediction-history";
  public static GET_CHATS = "/api/chat/get-chats";
  public static SEND_CHAT = "/api/chat/send-chat";
  public static GET_TEAMS = "/api/match/teams";
  public static GET_MATCH_PREDICTS = "/api/match/match-predicts";
  public static UPDATE_STANDINGS = "/api/Data/update-standings";
  public static UPDATE_MATCHES = "/api/Data/update-matches";
}
