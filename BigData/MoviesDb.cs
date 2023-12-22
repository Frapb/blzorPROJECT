using Azure.Core;
using MoviesData.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.Design;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Net;


namespace MoviesData
{
    public class MoviesDb
    {
        private static void ReacreateDb()
        {
            Parser parser = new Parser();
            parser.ReadInfo();

            using ApplicationContext db = new ApplicationContext();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Actors.AddRange(parser.actors.Values);
            db.SaveChanges();
            db.Tags.AddRange(parser.tags.Values);
            db.SaveChanges();
            db.Movies.AddRange(parser.movies.Values);
            db.SaveChanges();
        }
        public static List<Movie> GenerateSimilarForMovie(Movie movie)
        {
            using var db = new ApplicationContext();


            List<Movie> list = movie.Actors.SelectMany(actor => getActorById(actor.Id).Movies)
                            .Union(movie.Tags.SelectMany(tag => getTagById(tag.Id).Movies))
                            .Where(movie => !string.IsNullOrEmpty(movie.movieId))
                            .DistinctBy(movie => movie.movieId)
                            .ToList();

            if (list ==null || list.Count == 1 || list.Count == 0)
            {
                return new();
            }

           
            list.Remove(movie);

            var Comparer = new MovieSimco(movie);
            List<Movie> result = new();

            
            for (int i = 0; i < 10; i++)
            {
                Movie? maxMovie = list[0];
                foreach (var m in list)
                {
                    if (Comparer.Compare(m, maxMovie) == 1)
                    {
                        maxMovie = m;
                    }
                }
                list.Remove(maxMovie);
                result.Add(maxMovie);
            }

            return result;
        }

        public static Movie? getMovieById(string id)
        {
            using var db = new ApplicationContext();
            return db.Movies.Where(m => m.movieId == id)
                .Include(m => m.Actors)
                .Include(m => m.Tags)
                .FirstOrDefault();
        }

        
        public static Actor? getActorById(string id)
        {
            using var db = new ApplicationContext();
            return db.Actors.Where(a => a.Id == id)
                .Include(a => a.Movies)
                .FirstOrDefault();
        }

       
        public static Tag? getTagById(string id)
        {
            using var db = new ApplicationContext();
            return db.Tags.Where(t => t.Id == id)
                .Include(t => t.Movies)
                .FirstOrDefault();
        }

       
        public static List<Movie> FindMovies(string name, int quantity)
        {
            using var db = new ApplicationContext();
            return db.Movies
                .Where(m => (m.NameRU + m.NameUS).ToLower().Contains(name))
                .Take(quantity)
                .ToList();
        }

        
        public static List<Actor> FindActors(string name, int quantity)
        {
            using var db = new ApplicationContext();
            return db.Actors
                .Where(a => a.Name.ToLower().Contains(name.ToLower()))
                .Take(quantity)
                .ToList();
        }

       
        public static List<Tag> FindTags(string name, int quantity)
        {
            using var db = new ApplicationContext();
            return db.Tags
                .Where(t => t.Name.ToLower().Contains(name.ToLower()))
                .Take(quantity)
                .ToList();
        }

        public static void Main() { }

      

    }


}