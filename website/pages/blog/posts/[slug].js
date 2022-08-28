import Layout from '../../../components/layout';
import Date from '../../../components/date';
import Head from 'next/head';
import commonStyles from '../../../styles/common.module.css';
import styles from './post.module.css';
import { getAllPostSlugs, getPost } from '../../../lib/posts';

export default function Post({ post }) {
  return (
    <Layout>
      <Head>
        <title>{`${post.title} | Fluent Feeds`}</title>
      </Head>

      <section className={commonStyles.section}>
        <h1 className={styles.title}>{post.title}</h1>
        <small className={styles.info}>posted by {post.author} on <Date dateString={post.publishedTimestamp} /></small>
        <div className={styles.content} dangerouslySetInnerHTML={{ __html: post.content }} />
      </section>
    </Layout>
  );
}

export async function getStaticPaths() {
  const slugs = await getAllPostSlugs();
  const paths = slugs
    .map(slug => {
      return { params: { slug } };
    });
  return { paths, fallback: false };
}

export async function getStaticProps({ params }) {
  const post = await getPost(params.slug);
  return { props: { post } }
}
