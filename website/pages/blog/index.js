import Layout from '../../components/layout';
import Head from 'next/head';
import Link from 'next/link';
import commonStyles from '../../styles/common.module.css';
import { getAllPosts } from '../../lib/posts';
import { generateFeeds } from '../../lib/feed';

export default function Blog({ posts }) {
  return (
    <Layout>
      <Head>
        <title>Blog | Fluent Feeds</title>
      </Head>

      <section className={commonStyles.section}>
        <h1>Blog</h1>
      </section>

      <section className={commonStyles.section}>
        <ul>
          {posts.map(post => (
            <li>
              <Link href={`/blog/posts/${post.slug}`}>
                <a>
                  <h2>{post.title}</h2>
                  <small>posted by {post.author} on {post.timestamp}</small>
                  <p>{post.summary}</p>
                </a>
              </Link>
            </li>
          ))}
        </ul>
      </section>
    </Layout>
  )
}

export async function getStaticProps() {
  const posts = await getAllPosts();
  await generateFeeds(posts);
  return { props: { posts } };
}
