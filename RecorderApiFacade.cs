﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Configuration;
using Qualtrak.Coach.DataConnector.Core.Recorder;
using Qualtrak.Coach.DataConnector.Core.Recorder.Args;
using Qualtrak.Coach.DataConnector.Core.Shared;

namespace QATestRecorder
{
    public class RecorderApiFacade : IRecorderApiFacade
    {
        private readonly string _defaultFile;

        public RecorderApiFacade()
        {
            if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings["coach:service:defaultmediafile"]))
            {
                this._defaultFile = WebConfigurationManager.AppSettings["coach:service:defaultmediafile"];
            }
        }

        public async Task<IEnumerable<RecorderUser>> GetUsersAsync(DataConnectorProperties properties)
        {
            var list = new List<RecorderUser>();

            for (var i = 0; i < 1000; i++)
            {
                var user = new RecorderUser
                {
                    UserId = i.ToString(),
                    AccountId = i.ToString(),
                    Username = string.Format("agent.{0}", i),
                    FirstName = "Agent",
                    LastName = i.ToString(),
                    Mail = string.Format("agent.{0}@qualtrak.com", i)
                };

                list.Add(user);
            }

            return await Task.FromResult(list);
        }

        public async Task<IEnumerable<Media>> GetMediaForUserAsync(string userId, MediaForUserArgs args, DataConnectorProperties properties)
        {
            int recordingsProcessedSoFar = 0;
            var list = new List<Media>();

            for (var i = 0; i < 1000; i++)
            {
                var media = new Media
                {
                    RecorderUserId = userId,
                    Id = i.ToString(),
                    Date = DateTime.Now.AddDays(-1),
                    Metadata = "{ \"metadata\" : [{\"label\" : \"Claim No\", \"value\" : \"123456\", \"field\" : \"claim_no\", \"type\" : \"number\"},{\"label\" : \"Caller Id\", \"value\" : \"004401232312311\", \"field\" : \"caller_id\", \"type\" : \"number\" }, {\"label\" : \"Account No\", \"value\" : \"12121231314AB\", \"field\" : \"account_no\", \"type\" : \"string\" }, {\"label\" : \"A really long label description\", \"value\" : \"A really long piece of call metadata information 1234567890 1234567890\", \"field\" : \"notes\", \"type\" : \"string\" }] }",
                    FileName = _defaultFile ?? "a.wmv"
                };

                list.Add(media);
                recordingsProcessedSoFar++;

                if (recordingsProcessedSoFar > args.Limit) break;
            }

            return await Task.FromResult(list); ;
        }

        public async Task<IEnumerable<MediaUser>> GetMediaForUsersAsync(MediaForUsersArgs args, DataConnectorProperties properties)
        {
            var result = new List<MediaUser>();
            MediaForUserArgs userArgs = CreateMediaForUserArgs(args);

            foreach (var userId in args.UserIds)
            {
                IEnumerable<Media> media = await this.GetMediaForUserAsync(userId, userArgs, properties);
                IEnumerable<MediaUser> mediaUsers = media.Select(x => new MediaUser { MediaId = x.Id, RecorderUserId = x.RecorderUserId }).ToList();
                result.AddRange(mediaUsers);
                media.ToList().Clear();
                mediaUsers.ToList().Clear();
            }

            return result;
        }

        public async Task<IEnumerable<Media>> GetMediaByIdsAsync(IEnumerable<string> ids, DataConnectorProperties properties)
        {
            var list = new List<Media>();

            foreach (var recordingId in ids)
            {
                var media = new Media
                {
                    Id = recordingId,
                    Date = DateTime.Now.AddDays(-1),
                    Metadata = "{ \"metadata\" : [{\"label\" : \"Claim No\", \"value\" : \"123456\", \"field\" : \"claim_no\", \"type\" : \"number\"},{\"label\" : \"Caller Id\", \"value\" : \"004401232312311\", \"field\" : \"caller_id\", \"type\" : \"number\" }, {\"label\" : \"Account No\", \"value\" : \"12121231314AB\", \"field\" : \"account_no\", \"type\" : \"string\" }, {\"label\" : \"A really long label description\", \"value\" : \"A really long piece of call metadata information 1234567890 1234567890\", \"field\" : \"notes\", \"type\" : \"string\" }] }",
                    FileName = _defaultFile ?? "a.wmv"
                };

                list.Add(media);
            }

            return await Task.FromResult(list);
        }

        public async Task<string> GetMediaUrlAsync(string id, string originalUrl, DataConnectorProperties properties)
        {
            return await Task.FromResult(originalUrl);
        }

        public async Task SendEvaluationScoreAsync(SendEvaluationScoreArgs args, DataConnectorProperties properties)
        {
            throw new NotImplementedException();
        }

        public async Task<Stream> GetStreamAsync(string url)
        {
            Stream stream = new MemoryStream(1000000 * 10);

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            using (var client = new HttpClient() { BaseAddress = new Uri(url, UriKind.Absolute) })
            {
                try
                {
                    var response = client.GetByteArrayAsync("").Result;
                    stream = new MemoryStream(response);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return await Task.FromResult(stream);
        }

        private static MediaForUserArgs CreateMediaForUserArgs(MediaForUsersArgs args)
        {
            return new MediaForUserArgs
            {
                Limit = args.Limit,
                SearchCriteria = args.SearchCriteria,
                TimeZone = args.TimeZone
            };
        }
    }
}