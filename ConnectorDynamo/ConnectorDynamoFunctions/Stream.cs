﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Logging;

namespace Speckle.ConnectorDynamo.Functions
{
  public static class Stream
  {
    /// <summary>
    /// Get an existing Stream
    /// </summary>
    /// <param name="streamId">Id of the Stream to get</param>
    /// <param name="account">Speckle account to use, if not provided the default account will be used</param>
    /// <returns name="stream">A Stream</returns>
    // [NodeCategory("Create")]
    // public static object Get([ArbitraryDimensionArrayImport] object streamId,
    //   [DefaultArgument("null")] Core.Credentials.Account account = null)
    // {
    //   Tracker.TrackPageview(Tracker.STREAM_GET);
    //
    //   var streamIds = Utils.MultiDimensionalInputToList<string>(streamId);
    //   if (!streamIds.Any())
    //   {
    //     Log.CaptureAndThrow(new Exception("Please provide one or more Stream Ids."));
    //   }
    //   else if (streamIds.Count > 20)
    //   {
    //     Log.CaptureAndThrow(new Exception("Please provide less than 20 Stream Ids."));
    //   }
    //
    //   var streams = new List<StreamWrapper>();
    //   try
    //   {
    //     foreach (var id in streamIds)
    //     {
    //       if (account == null)
    //         account = AccountManager.GetDefaultAccount();
    //
    //       var client = new Client(account);
    //
    //       //Exists?
    //       Core.Api.Stream res = client.StreamGet(id).Result;
    //       streams.Add(new StreamWrapper(res.id, account.id, account.serverInfo.url));
    //     }
    //   }
    //   catch (Exception e)
    //   {
    //     if (e.InnerException!=null)
    //     {
    //       if (e.InnerException is HttpRequestException)
    //         throw new Exception("Could not reach the server, is it online?");
    //       throw e.InnerException;
    //     }
    //   }
    //
    //   if (streams.Count() == 1)
    //     return streams[0];
    //
    //   return streams;
    // }

    [IsVisibleInDynamoLibrary(false)]
    public static StreamWrapper GetByStreamAndAccountId(string streamId, string accountId)
    {
      var account = AccountManager.GetAccounts().FirstOrDefault(x => x.id == accountId);
      var client = new Client(account);

      //Exists?
      Core.Api.Stream res = client.StreamGet(streamId).Result;

      return new StreamWrapper(res.id, account.id, account.serverInfo.url);
    }


    /// <summary>
    /// Update a Stream details, use is limited to 1 stream at a time
    /// </summary>
    /// <param name="stream">Stream object to update</param>
    /// <param name="name">Name of the Stream</param>
    /// <param name="description">Description of the Stream</param>
    /// <param name="isPublic">True if the stream is to be publicly available</param>
    /// <returns name="stream">Updated Stream object</returns>
    public static StreamWrapper Update(StreamWrapper stream,
      [DefaultArgument("null")] string name,
      [DefaultArgument("null")] string description, [DefaultArgument("null")] bool? isPublic)
    {
      Tracker.TrackPageview(Tracker.STREAM_UPDATE);
      
      if (stream == null)
      {
        Core.Logging.Log.CaptureAndThrow(new Exception("Invalid stream."));
      }

      if (name == null && description == null && isPublic == null)
        return null;

      var account = stream.GetAccount();

      var client = new Client(account);

      var input = new StreamUpdateInput {id = stream.StreamId};

      if (name != null)
        input.name = name;

      if (description != null)
        input.description = description;

      if (isPublic != null)
        input.isPublic = (bool) isPublic;


      var res = client.StreamUpdate(input).Result;

      if (res)
        return stream;

      return null;
    }

    /// <summary>
    /// Extracts the details of a given stream, use is limited to max 20 streams 
    /// </summary>
    /// <param name="stream">Stream object</param>
    [NodeCategory("Query")]
    [MultiReturn(new[]
    {
      "id", "name", "description", "createdAt", "updatedAt", "isPublic", "collaborators", "branches"
    })]
    public static object Details([ArbitraryDimensionArrayImport] object stream)
    {
      Tracker.TrackPageview(Tracker.STREAM_DETAILS);

      var streams = Utils.InputToStream(stream);

      if (!streams.Any())
      {
        Log.CaptureAndThrow(new Exception("Please provide one or more Streams."));
      }
      else if (streams.Count > 20)
      {
        Log.CaptureAndThrow(new Exception("Please provide less than 20 Streams."));
      }

      var details = new List<Dictionary<string, object>>();

      foreach (var streamWrapper in streams)
      {
        var account = streamWrapper.GetAccount();

        var client = new Client(account);

        Core.Api.Stream res = client.StreamGet(streamWrapper.StreamId).Result;

        details.Add(new Dictionary<string, object>
        {
          {"id", res.id},
          {"name", res.name},
          {"description", res.description},
          {"createdAt", res.createdAt},
          {"updatedAt", res.updatedAt},
          {"isPublic", res.isPublic},
          {"collaborators", res.collaborators},
          {"branches", res.branches?.items}
        });
      }

      if (details.Count() == 1)
        return details[0];

      return details;
    }

    /// <summary>
    /// List all your Streams
    /// </summary>
    /// <param name="account">Speckle account to use, if not provided the default account will be used</param>
    /// <param name="limit">Max number of streams to get</param>
    /// <returns name="streams">Your Streams</returns>
    [NodeCategory("Query")]
    public static List<StreamWrapper> List([DefaultArgument("null")] Core.Credentials.Account account = null,
      [DefaultArgument("10")] int limit = 10)
    {
      Tracker.TrackPageview(Tracker.STREAM_LIST);

      if (account == null)
        account = AccountManager.GetDefaultAccount();

      var client = new Client(account);
      var res = client.StreamsGet(limit).Result;

      var streamWrappers = new List<StreamWrapper>();
      res.ForEach(x => { streamWrappers.Add(new StreamWrapper(x.id, account.id, account.serverInfo.url)); });

      return streamWrappers;
    }
  }
}
