using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DataAugmentation;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.IndexerSearchTests
{
    public class GetSearchTitleFixture : CoreTest<TestSearch>
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .With(s => s.Title = "Hawaii Five-0")
                    .Build();
        }

        private void WithSceneMapping()
        {
            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetSceneName(_series.Id, -1))
                  .Returns("Hawaii Five 0 2010");
        }

        [Test]
        public void should_return_series_title_when_there_is_no_scene_mapping()
        {
            Subject.GetSearchTitle(_series, 5).Should().Be(_series.Title);
        }

        [Test]
        public void should_return_scene_mapping_when_one_exists()
        {
            WithSceneMapping();

            Subject.GetSearchTitle(_series, 5).Should().Be("Hawaii Five 0 2010");
        }

        [Test]
        public void should_return_season_scene_name_when_one_exists()
        {
            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetSceneName(_series.Id, 5))
                  .Returns("Hawaii Five 0 2010 - Season 5");

            Subject.GetSearchTitle(_series, 5).Should().Be("Hawaii Five 0 2010 - Season 5");
        }

        [Test]
        public void should_return_series_scene_name_when_one_for_season_does_not_exist()
        {
            WithSceneMapping();

            Subject.GetSearchTitle(_series, 5).Should().Be("Hawaii Five 0 2010");
        }

        [Test]
        public void should_replace_ampersand_with_and()
        {
            _series.Title = "Franklin & Bash";

            Subject.GetSearchTitle(_series, 5).Should().Be("Franklin and Bash");
        }

        [TestCase("Betty White's Off Their Rockers", "Betty Whites Off Their Rockers")]
        [TestCase("Star Wars: The Clone Wars", "Star Wars The Clone Wars")]
        [TestCase("Hawaii Five-0", "Hawaii Five-0")]
        public void should_replace_some_special_characters(string input, string expected)
        {
            _series.Title = input;

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.GetSceneName(_series.Id, -1))
                  .Returns("");

            Subject.GetSearchTitle(_series, 5).Should().Be(expected);
        }
    }
}
