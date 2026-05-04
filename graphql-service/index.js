import { ApolloServer } from '@apollo/server';
import { startStandaloneServer } from '@apollo/server/standalone';
import pg from 'pg';
import { typeDefs } from './schema.js';
import { resolvers } from './resolvers.js';

const { Pool } = pg;

const db = new Pool({ connectionString: process.env.DATABASE_URL });

const server = new ApolloServer({ typeDefs, resolvers });

const { url } = await startStandaloneServer(server, {
  listen: { port: 4000 },
  context: async () => ({ db }),
});

console.log(`GraphQL server ready at ${url}`);
