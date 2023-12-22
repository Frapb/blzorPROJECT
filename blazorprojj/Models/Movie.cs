using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MoviesData.Models
{
    public class Movie
    {
        public string? NameRU { get; set; }
        [NotMapped]
        public string ImageUrl { get; set; }
       
        public string? NameUS { get; set; }
        public List<Actor> Actors = new();
        public List<Tag> Tags = new();
        public double? Rate { get; set; }
        public string? imdbId { get; set; }
        [Key] public string movieId { get; set; }


        
        public double Similarity(Movie movie)
        {
            double actorsIntersect = Actors.Select(a => a.Id)
                .Intersect(movie.Actors.Select(a => a.Id))
                .Count();
            
            double actorsUnion = Actors.Count;

            double tagsIntersect = Tags.Select(t => t.Id)
                .Intersect(movie.Tags.Select(t => t.Id))
                .Count();
            
            double tagsUnion = Tags.Count;

            double actorsRate; 
            if (actorsUnion != 0)
                actorsRate = actorsIntersect / actorsUnion;
            else
                actorsRate = 0;

            double tagsRate; 
            if (tagsUnion != 0)
                tagsRate = tagsIntersect / tagsUnion;
            else
                tagsRate = 0;

            double rate = (actorsRate + tagsRate) * 0.5;

            double anotherMovieRate = 0; 

            if (movie.Rate != null)
            {
                anotherMovieRate = (double)movie.Rate;
            }

            
            double resultRate = rate * 0.9 + (anotherMovieRate * 0.1) * 0.1;
            

            return resultRate;
        }
        public Movie()
        {

        }
        public async void GetMovieImageUrl()
        {
            using (WebClient client = new WebClient())
            {
                string url = $"https://www.omdbapi.com/?i={imdbId}&apikey=c45ce5e";
                string info = client.DownloadString(url);
                int start = info.IndexOf("https://");
                int end = info.IndexOf(".jpg")+4;
                ImageUrl = info.Substring(start,end-start); 

            }
        }
    }
    public class MovieSimco : IComparer<Movie>
    {
        public Movie mainMovie { get; private set; }
        public MovieSimco(Movie mainMovie)
        {
            this.mainMovie = mainMovie;
        }
        public int Compare(Movie? x, Movie? y)
        {
            if (mainMovie.Similarity(x) < mainMovie.Similarity(y))
                return -1;
            else if (mainMovie.Similarity(x) > mainMovie.Similarity(y))
                return 1;
            else
                return 0;
        }
       
    }
}
