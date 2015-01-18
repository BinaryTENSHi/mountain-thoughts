using TweetSharp;

namespace mountain_thoughts
{
    public static class Twitter
    {
        private static TwitterService _service;

        public static void Authenticate()
        {
            _service = new TwitterService(TwitterSecrets.ConsumerKey, TwitterSecrets.ConsumerSecret);
            _service.AuthenticateWith(TwitterSecrets.AccessToken, TwitterSecrets.AccessSecret);
        }

        public static void Tweet(string content)
        {
            _service.SendTweet(new SendTweetOptions {Status = content});
        }
    }
}