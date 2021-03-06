using System;
using System.Collections.Generic;
using System.Linq;

using jp.tamagotchi.data.DataAccess;
using jp.tamagotchi.data.Entities;

using MongoDB.Driver;

namespace jp.tamagotchi.data.Queries
{
    public class GetUserByExampleQuery : IQuery<GetUserByExampleQueryPayload, GetUserByExampleQueryResult>
    {

        private MySQLContext _mySqlContext;
        private MongoDBContext _mongoDBContext;

        public GetUserByExampleQuery(MySQLContext mySqlContext, MongoDBContext mongoDBContext)
        {
            _mySqlContext = mySqlContext;
            _mongoDBContext = mongoDBContext;
        }

        public GetUserByExampleQueryResult Query(GetUserByExampleQueryPayload payload)
        {

            var result = new GetUserByExampleQueryResult();

            Func<User, bool> predicate = entity =>
                (payload.Example?.Id ?? 0) == entity.Id &&
                (payload.Example?.UserName ?? "") == entity.UserName &&
                (payload.Example?.Password ?? "") == entity.Password &&
                (payload.Example?.Email ?? "") == entity.Email;

            Func<User, List<PetOwnership>> getPets = user => _mongoDBContext.GetCollection<PetOwnership>("petOwnerships")
                .AsQueryable()
                .Where(document => document.UserId == user.Id)
                .ToList();

            try
            {
                var data = _mySqlContext.User.Where(predicate)
                    .Select(user => new GetUserByExampleQueryResultData() { User = user, Pets = getPets(user) });

                result.Data = (payload.Size != 0 ? data.Take(payload.Size) : data).ToList();
            }
            catch (System.Exception ex)
            {
                result.AddError(ex);
            }

            return result;

        }
    }

    public class GetUserByExampleQueryPayload
    {
        public int Size { get; set; }
        public User Example { get; set; }
    }

    public class GetUserByExampleQueryResult : DataQueryResult<List<GetUserByExampleQueryResultData>> { }

    public class GetUserByExampleQueryResultData
    {
        public User User { get; set; }
        public List<PetOwnership> Pets { get; set; }
    }

}