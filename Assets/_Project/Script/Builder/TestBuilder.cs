using System.Collections.Generic;

namespace MeowStudio.Utils
{
    public class TestBuilder
    {
        private string name;
        private float health;
        private float damage;

        //test use
        TestBuilder test = new TestBuilder.Builder()
            .WithName("test")
            .WithHealth(10)
            .WithDamage(1)
            .Build();

        public class Builder
        {
            //default values
            string name = "Test item";
            float health = 10;
            float damage = 1;

            public Builder WithName(string name)
            {
                this.name = name;
                return this;
            }
            public Builder WithHealth(float health)
            {
                this.health = health;
                return this;
            }
            public Builder WithDamage(float damage)
            {
                this.damage = damage;
                return this;
            }

            public TestBuilder Build()
            {
                var test = new TestBuilder();

                test.name = name;
                test.health = health;
                test.damage = damage;

                return test;
            }
        }
    }
}
