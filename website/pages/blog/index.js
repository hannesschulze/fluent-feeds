import Layout from '../../components/layout';
import Date from '../../components/date';
import Head from 'next/head';
import Link from 'next/link';
import commonStyles from '../../styles/common.module.css';
import styles from './index.module.css';
import { getAllPosts } from '../../lib/posts';
import { generateFeeds } from '../../lib/feed';

export default function Blog({ posts }) {
  return (
    <Layout>
      <Head>
        <title>Blog | Fluent Feeds</title>
      </Head>

      <section className={`${styles.hero} ${commonStyles.section}`}>
        <h1>Blog</h1>
        <h2 className='tagline'>News about the app.</h2>
      </section>

      <section className={`${styles.posts} ${commonStyles.section}`}>
        <ul>
          {posts.map(post => (
            <li key={post.slug}>
              <Link href={`/blog/posts/${post.slug}`}>
                <a>
                  <h2 className={styles.title}>{post.title}</h2>
                  <small className={styles.info}>posted by {post.author} on <Date dateString={post.publishedTimestamp} /></small>
                  <p className={styles.summary}>{post.summary}</p>
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
